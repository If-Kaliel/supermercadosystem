# Infraestrutura MySQL, Docker e EF Core

Diagnóstico e correção executados em 08/10/2026, na main.

## Causa comprovada

Havia configurações diferentes para a mesma conexão. O appsettings.json local havia sido alterado para o banco SuperMarket e uma credencial diferente da aceita pelo container. O appsettings.Development.json apontava para Supermercado, sem senha, e User Secrets continha a conexão válida.

Na reprodução controlada antes da correção, a API em Production retornou HTTP 500 com erro de autenticação. Em Development, usando User Secrets, a mesma consulta retornou HTTP 200. O processo anterior do Rider já estava encerrado, portanto sua origem exata de configuração não pôde ser recuperada; a divergência e o erro foram reproduzidos pelos dois modos de execução.

O container TDSPB estava funcionando, com MySQL 26.7.0 na porta 3306 e volume persistente. O usuário root com host % estava desbloqueado e usava caching_sha2_password. A conexão válida funcionou do Windows para a porta publicada pelo Docker. Não foi necessário trocar o provider, alterar usuários, desativar autenticação ou recriar o banco.

## Correção

- Program.cs carrega appsettings.Local.json somente em Development, preserva a precedência de argumentos e variáveis de ambiente e desconsidera conexões vazias. O log inicial informa a origem efetiva e o destino, sem senha.
- Os arquivos appsettings compartilhados ficaram sem credenciais ou conexões concorrentes. O exemplo local contém apenas um marcador de senha.
- Os perfis http e https definem os dois indicadores de ambiente como Development.
- O projeto API exclui os arquivos locais da publicação. appsettings.Local.json já é ignorado pelo Git.
- scripts/Iniciar-Desenvolvimento.ps1 prepara SDK, Docker, pacotes, migrations e validações. A preparação impede recompilar enquanto a API está em execução no Rider, evitando bloqueio dos assemblies no Windows. A conexão antiga de User Secrets foi transferida para o arquivo local e sua chave removida somente após validar EF Core. Outros segredos são preservados.
- O container e seu volume foram reaproveitados; foi configurado reinício unless-stopped. O script nunca remove containers, volumes ou bancos.

O destino deste ambiente é TDSPB, 127.0.0.1:3306, banco Supermercado. O nome SuperMarket do exemplo da aula não é o banco usado pelo projeto.

## Resultados reais

| Verificação | Resultado |
| --- | --- |
| dotnet restore e dotnet tool restore | Sucesso com SDK 10.0.400 |
| dotnet build | Zero erros e avisos |
| dotnet test | Executado com sucesso; não há projetos de framework de testes nesta solução |
| Programa de integração | As 16 verificações passaram; transação revertida |
| EF database update pelo Windows | Sucesso; as duas migrations já estavam aplicadas |
| EF has-pending-model-changes | Sem alterações pendentes |
| Esquema | Nove tabelas de entidades e histórico de migrations presentes |
| API temporária com perfil http | GET /categorias retornou 200; logs sem erros de autenticação/conexão |
| Conexão local sem variável de ambiente | HTTP 200; origem appsettings.Local.json registrada |
| Variável de conexão vazia | HTTP 200; conexão local preservada |
| Variável de conexão válida | HTTP 200; origem EnvironmentVariablesConfigurationProvider registrada |
| Production sem conexão externa | Falha imediata com instrução clara de configuração |
| Publicação da API | appsettings.Local.json ausente da saída |
| Execução no Rider | Perfil Supermercado.Api: http iniciado; GET /categorias retornou 200 |
| Reinício real do MySQL | A mesma API do Rider voltou a consultar com HTTP 200 |
| Preservação | Mesmo ID de container, mesmo volume e mesmos checksums das nove tabelas e histórico após os testes |
| Segunda inicialização | Sucesso; arquivo de conexão local preservado exatamente |
| Cópia sem configuração local nem chave User Secrets | Script configurou a conexão pelo container; build, 16 verificações e HTTP 200 aprovados no Windows PowerShell 5.1 |
| API em execução durante a preparação | Pré-verificação retorna instrução clara para parar o perfil antes de recompilar |

Nenhuma migration foi criada, removida ou alterada. O programa de integração continua separado das regras de negócio e não grava dados permanentes de teste. O domínio Supermercado foi mantido conforme a autorização do professor informada pelo grupo anteriormente.

## Uso no Rider

Com Docker Desktop iniciado, execute a preparação uma vez pelo script descrito no README. Depois use Supermercado.Api: http ou https no Rider. A conexão local é reutilizada automaticamente. Para revalidar tudo, o script aceita -Verificar; para iniciar também a API pelo terminal, aceita -ExecutarApi.
