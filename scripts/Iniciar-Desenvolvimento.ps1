[CmdletBinding()]
param(
    [string]$ContainerName = 'TDSPB',
    [int]$Port = 3306,
    [string]$Database = 'Supermercado',
    [switch]$AoAbrirProjeto,
    [switch]$Verificar,
    [switch]$ExecutarApi
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$repoRoot = Split-Path $PSScriptRoot -Parent
$localPath = Join-Path $repoRoot 'src\Supermercado.Api\appsettings.Local.json'
$secretId = 'da7b2972-9ecf-4f1a-bbcc-2324c1bdd08f'
$secretPath = Join-Path $env:APPDATA "Microsoft\UserSecrets\$secretId\secrets.json"
# Imagem MySQL 26.7.0 validada neste projeto; o digest evita mudanças de mysql:latest.
$mysqlImage = 'mysql@sha256:9d48c42f8341068f199116dfccb919b607c99765b5c61e549a548a43033471a4'

function Invoke-DotNet {
    param([string[]]$Arguments)
    & $script:dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) falhou. Consulte a mensagem acima." }
}

Push-Location $repoRoot
try {
    $userDotnet = Join-Path $env:USERPROFILE '.dotnet\dotnet.exe'
    $script:dotnet = if (Test-Path $userDotnet) { $userDotnet } else { (Get-Command dotnet).Source }
    $env:DOTNET_ROOT = Split-Path $script:dotnet -Parent
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
    $sdk = & $script:dotnet --version
    if ($LASTEXITCODE -ne 0 -or $sdk -notmatch '^10\.') { throw 'Instale o SDK .NET 10 para esta solução.' }
    Write-Host "SDK selecionado: $sdk"
    $runningApi = Get-CimInstance Win32_Process -Filter "Name = 'Supermercado.Api.exe'" | Where-Object {
        $_.ExecutablePath -and $_.ExecutablePath.StartsWith($repoRoot + '\', [StringComparison]::OrdinalIgnoreCase)
    }
    if ($runningApi -and $AoAbrirProjeto) {
        Write-Host 'A API deste projeto já está em execução. Preparação automática dispensada.'
        return
    }
    if ($runningApi) {
        throw 'Pare a execução de Supermercado.Api no Rider antes de preparar ou recompilar o ambiente.'
    }

    $null = & docker info --format '{{.ServerVersion}}' 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'O engine Linux do Docker Desktop precisa estar iniciado.' }
    $containerJson = & docker container inspect $ContainerName 2>$null
    $container = if ($LASTEXITCODE -eq 0) { ($containerJson | ConvertFrom-Json)[0] } else { $null }
    if ($null -ne $container -and $container.Config.Image -notmatch '^mysql(?::|@)') {
        throw 'O nome escolhido pertence a outro serviço. Use -ContainerName e -Port para um ambiente separado.'
    }

    $connection = $null
    if (Test-Path $localPath) {
        $connection = (Get-Content $localPath -Raw | ConvertFrom-Json).ConnectionStrings.MySql
    }
    if ([string]::IsNullOrWhiteSpace($connection) -and (Test-Path $secretPath)) {
        $connection = (Get-Content $secretPath -Raw | ConvertFrom-Json).'ConnectionStrings:MySql'
    }
    $settings = New-Object System.Data.Common.DbConnectionStringBuilder
    if (-not [string]::IsNullOrWhiteSpace($connection)) {
        $settings.set_ConnectionString($connection)
        if ($settings['Server'] -notin @('127.0.0.1', 'localhost') -or
            [int]$settings['Port'] -ne $Port -or $settings['Database'] -cne $Database) {
            throw 'A conexão local aponta para outro destino. Confira -Port e -Database antes de aplicar migrations.'
        }
    } else {
        $password = if ($null -ne $container) {
            ($container.Config.Env | Where-Object { $_ -like 'MYSQL_ROOT_PASSWORD=*' }) -replace '^MYSQL_ROOT_PASSWORD=', ''
        } else {
            $bytes = New-Object byte[] 32
            $random = [Security.Cryptography.RandomNumberGenerator]::Create()
            try { $random.GetBytes($bytes) } finally { $random.Dispose() }
            [Convert]::ToBase64String($bytes)
        }
        if ([string]::IsNullOrWhiteSpace($password)) {
            throw 'Credencial do container existente indisponível. Preserve-o e use um ambiente separado com -ContainerName e -Port.'
        }
        $settings['Server'] = '127.0.0.1'
        $settings['Port'] = [string]$Port
        $settings['Database'] = $Database
        $settings['User'] = 'root'
        $settings['Password'] = $password
        $connection = $settings.get_ConnectionString()
    }
    if ([string]::IsNullOrWhiteSpace([string]$settings['Password'])) { throw 'A conexão local precisa de uma senha válida.' }
    if (-not [string]::IsNullOrWhiteSpace($env:ConnectionStrings__MySql) -and $env:ConnectionStrings__MySql -cne $connection) {
        throw 'ConnectionStrings__MySql está sobrescrevendo a configuração local. Remova o conflito antes de continuar.'
    }

    if ($null -eq $container) {
        if (Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue) {
            throw "Porta $Port ocupada. Use -ContainerName supermercado-mysql -Port 3307 para criar um ambiente separado."
        }
        $previousPassword = $env:MYSQL_ROOT_PASSWORD
        try {
            $env:MYSQL_ROOT_PASSWORD = [string]$settings['Password']
            & docker run --name $ContainerName --restart unless-stopped -e MYSQL_ROOT_PASSWORD `
                -p "127.0.0.1:${Port}:3306" -v "${ContainerName}-dados:/var/lib/mysql" -d $mysqlImage
            if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar o container. Nenhum recurso existente foi removido.' }
        } finally { $env:MYSQL_ROOT_PASSWORD = $previousPassword }
    } else {
        $bindings = $container.HostConfig.PortBindings.'3306/tcp'
        if (-not ($bindings | Where-Object { $_.HostPort -eq [string]$Port -and $_.HostIp -in @('', '0.0.0.0', '127.0.0.1') })) {
            throw 'O container existente não publica a porta esperada para o Windows. Preserve-o e use um ambiente separado.'
        }
        & docker update --restart unless-stopped $ContainerName | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar a política de reinício.' }
        if (-not $container.State.Running) {
            & docker start $ContainerName
            if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar o container existente.' }
        }
    }
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        & docker exec $ContainerName sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysqladmin ping --silent' 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw 'MySQL não ficou pronto dentro de 60 segundos. O container e seus dados foram preservados.' }

    $local = @{ ConnectionStrings = @{ MySql = $connection } } | ConvertTo-Json -Depth 4
    [IO.File]::WriteAllText($localPath, $local + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Invoke-DotNet -Arguments @('restore')
    Invoke-DotNet -Arguments @('tool', 'restore')
    Invoke-DotNet -Arguments @('build', '--no-restore')
    # Esta conexão parte do processo .NET no Windows, pela porta publicada do Docker.
    Invoke-DotNet -Arguments @('ef', 'database', 'update', '--project', 'src/Supermercado.Infrastructure',
        '--startup-project', 'src/Supermercado.Api', '--no-build', '--', '--environment', 'Development')
    Invoke-DotNet -Arguments @('ef', 'migrations', 'has-pending-model-changes', '--project',
        'src/Supermercado.Infrastructure', '--startup-project', 'src/Supermercado.Api', '--no-build',
        '--', '--environment', 'Development')
    if (Test-Path $secretPath) {
        $oldSecrets = Get-Content $secretPath -Raw | ConvertFrom-Json
        if ($null -ne $oldSecrets.'ConnectionStrings:MySql') {
            Invoke-DotNet -Arguments @('user-secrets', 'remove', 'ConnectionStrings:MySql', '--id', $secretId)
        }
    }

    if ($Verificar) {
        Invoke-DotNet -Arguments @('test', '--no-build', '--verbosity', 'minimal')
        $previousConnection = $env:ConnectionStrings__MySql
        $previousLogLevel = $env:Logging__LogLevel__Default
        try {
            $env:ConnectionStrings__MySql = $connection
            $env:Logging__LogLevel__Default = 'Warning'
            Invoke-DotNet -Arguments @('run', '--project', 'tests/Supermercado.PersistenceChecks', '--no-build')
        } finally {
            $env:ConnectionStrings__MySql = $previousConnection
            $env:Logging__LogLevel__Default = $previousLogLevel
        }
        # Porta livre para testar a API sem ocupar a porta usada pelo Rider.
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $listener.Start()
        $apiPort = $listener.LocalEndpoint.Port
        $listener.Stop()
        $logPrefix = Join-Path $env:TEMP ('supermercado-api-' + [Guid]::NewGuid().ToString('N'))
        $api = Start-Process -FilePath $script:dotnet -ArgumentList @('run', '--project',
            'src/Supermercado.Api', '--no-build', '--launch-profile', 'http', '--urls', "http://127.0.0.1:$apiPort") `
            -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru `
            -RedirectStandardOutput "$logPrefix.out.log" -RedirectStandardError "$logPrefix.err.log"
        try {
            $response = $null
            for ($attempt = 0; $attempt -lt 30; $attempt++) {
                $api.Refresh()
                if ($api.HasExited) { throw 'A API encerrou durante a verificação. Confira os logs no diretório temporário.' }
                try {
                    $response = Invoke-WebRequest "http://127.0.0.1:$apiPort/categorias" -UseBasicParsing -TimeoutSec 2
                    break
                } catch { Start-Sleep -Milliseconds 500 }
            }
            if ($null -eq $response -or [int]$response.StatusCode -ne 200) {
                throw 'GET /categorias não respondeu com HTTP 200 pela conexão Windows -> Docker.'
            }
            $logs = (Get-Content "$logPrefix.out.log", "$logPrefix.err.log" -Raw) -join "`n"
            if ($logs -match 'Access denied for user|Authentication to host|Unable to connect|Unhandled exception') {
                throw 'A API registrou erro de autenticação/conexão durante a verificação.'
            }
            Write-Host 'GET /categorias: HTTP 200. Logs sem erros de autenticação ou conexão.'
        } finally {
            Get-CimInstance Win32_Process | Where-Object {
                $_.ParentProcessId -eq $api.Id -and $_.Name -eq 'Supermercado.Api.exe'
            } | ForEach-Object { Stop-Process -Id $_.ProcessId -ErrorAction SilentlyContinue }
            Stop-Process -Id $api.Id -ErrorAction SilentlyContinue
        }
        Write-Host 'Verificações de persistência concluídas; os registros de teste foram revertidos.'
    }
    Write-Host "MySQL disponível em 127.0.0.1:$Port; banco $Database; container $ContainerName."
    Write-Host 'Conexão salva somente em appsettings.Local.json (ignorado pelo Git).'
    if ($ExecutarApi) {
        Invoke-DotNet -Arguments @('run', '--project', 'src/Supermercado.Api', '--no-build', '--launch-profile', 'http')
    } else {
        Write-Host 'No Rider, execute o perfil http ou https de Supermercado.Api.'
    }
} finally {
    Pop-Location
}
