# SupermercadoSystem — CP2

Projeto de persistência do Checkpoint 2 de .NET, continuação do MER e das entidades do CP1.

## Integrantes — 2TDSPB

| Nome | RM |
| --- | --- |
| Kaliel Aquino | 567587 |
| Andre Matuda | 566733 |
| Paulo Diedrich | 567618 |

## Domínio e escopo

Sistema de Gestão de Supermercado e Frente de Caixa (PDV): catálogo de produtos, fornecedores, clientes, funcionários, caixas, vendas, itens e pagamentos.

**Adaptação do enunciado:** o material do CP2 usa Recommenda como domínio de referência. Esta solução continua o domínio Supermercado do CP1, conforme a definição do grupo. Não inclui filmes, séries ou herança TPH; também não existe relação 1:1 no MER original. Caso o professor exija especificamente Recommenda ou essas cardinalidades, essa diferença precisa ser alinhada antes da entrega.

O CP2 adiciona Entity Framework Core, MySQL, mapeamento das nove entidades, uma migration inicial, repositório genérico e injeção de dependência. A API oferece endpoints de categorias para demonstrar a persistência; não é um PDV completo com regras fiscais ou fechamento de venda.

## Organização

- `Supermercado.Domain`: nove entidades do CP1, sem dependência de EF Core.
- `Supermercado.Application`: contrato `IRepository<T>`.
- `Supermercado.Infrastructure`: `SupermercadoContext`, configurações Fluent API, migration e `Repository<T>`.
- `Supermercado.Api`: configuração, endpoints e registro de DI no `Program.cs`.
- `docs/`: MER do CP1, diagrama físico e SQL gerado pela migration.

O contexto é registrado como scoped por `AddDbContext` e o repositório por `AddScoped`. Todos os repositórios de uma requisição compartilham o mesmo contexto. `AddAsync`, `Update` e `Remove` precisam de `SaveChangesAsync` para gravar. `ListAsync` consulta sem tracking; `GetByIdAsync` permite editar a entidade rastreada.

## Requisitos

- SDK .NET 10. O `global.json` aceita as feature bands estáveis de .NET 10.
- Docker Desktop com engine Linux ativo e porta 3306 disponível.
- MySQL em Docker, imagem `mysql:latest`, seguindo o padrão da aula.
- EF Core 10.0.11, ferramenta local `dotnet-ef` 10.0.11 e provider oficial `MySql.EntityFrameworkCore` 10.0.9.

A compatibilidade do provider com .NET 10 está descrita no [NuGet oficial do MySQL](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.9).

## Subir o banco e configurar a conexão

Execute os comandos abaixo no **PowerShell**, na raiz do repositório. Escolha uma senha exclusivamente de desenvolvimento. A senha fica no ambiente e em User Secrets, fora do Git.

```powershell
$env:MYSQL_ROOT_PASSWORD = Read-Host "Senha do MySQL local"
docker run --name TDSPB -e MYSQL_ROOT_PASSWORD -p 3306:3306 -d mysql:latest
```

Se o container já existir, use `docker start TDSPB` e informe a senha com que ele foi criado. Não execute novamente `docker run` com o mesmo nome.

Aguarde o MySQL ficar pronto. Para conferir:

```powershell
docker logs TDSPB --tail 30
```

Restaure os pacotes e a ferramenta local, e configure a connection string:

```powershell
dotnet restore
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:MySql" "Server=127.0.0.1;Port=3306;Database=Supermercado;User=root;Password=$env:MYSQL_ROOT_PASSWORD;" --project src/Supermercado.Api
```

O `appsettings.Development.json` contém servidor, porta e nome do banco, sem senha. Em Development, o ASP.NET Core carrega os User Secrets automaticamente. Para outro ambiente, configure `ConnectionStrings__MySql` no ambiente do servidor. Não coloque credenciais reais em arquivos versionados.

Se `dotnet --list-sdks` mostrar apenas um SDK antigo, confira o PATH. Neste computador, o SDK 10 também está em `$env:USERPROFILE\.dotnet\dotnet.exe`; a instalação de `Program Files` contém um SDK anterior. Se necessário, nesta sessão:

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet --list-sdks
```

## Aplicar a migration

```powershell
dotnet ef database update --project src/Supermercado.Infrastructure --startup-project src/Supermercado.Api -- --environment Development
dotnet build
```

Há somente uma migration, `InitialCreate`, que cria o esquema completo. Não é necessário gerar outra migration para iniciar o projeto. `database update` pode ser executado novamente: o EF consulta `__EFMigrationsHistory` e aplica somente migrations pendentes.

Para verificar o esquema:

```powershell
docker exec -e MYSQL_PWD=$env:MYSQL_ROOT_PASSWORD TDSPB mysql -uroot Supermercado -e "SHOW TABLES;"
dotnet ef migrations has-pending-model-changes --project src/Supermercado.Infrastructure --startup-project src/Supermercado.Api -- --environment Development
```

## Executar e demonstrar a API

```powershell
dotnet run --project src/Supermercado.Api --launch-profile http
```

Em outro PowerShell:

```powershell
Invoke-RestMethod http://localhost:5278/categorias
Invoke-RestMethod http://localhost:5278/categorias -Method Post -ContentType "application/json" -Body '{"nome":"Mercearia","descricao":"Alimentos e itens de despensa"}'
```

- `GET /categorias`: lista registros do MySQL.
- `GET /categorias/{id}`: retorna a categoria ou 404.
- `POST /categorias`: retorna 201 e Location; dados inválidos retornam 400 e nome duplicado retorna 409.
- `GET /openapi/v1.json`: documento OpenAPI em Development.
- Os exemplos também estão em `src/Supermercado.Api/Supermercado.Api.http`, executáveis pelo Rider.

O perfil HTTPS original continua disponível. O perfil HTTP é suficiente para demonstração local.

## Modelo físico

Todas as PKs são GUIDs gerados no Domain e persistidos como `char(36)`. Valores monetários usam `decimal(18,2)`; strings têm tamanho e obrigatoriedade definidos. CPF/CNPJ devem ser informados somente com dígitos; a validação cadastral não faz parte deste CP2. As datas de venda e pagamento são geradas em UTC pela aplicação; MySQL `datetime(6)` não armazena fuso.

| Relacionamento | Opcionalidade / exclusão |
| --- | --- |
| Categoria → Produto | Categoria obrigatória; exclusão restrita quando há produtos |
| Fornecedor → Produto | Fornecedor obrigatório; pode existir sem produtos; exclusão restrita |
| Cliente → Venda | Cliente opcional; exclusão do cliente deixa a FK nula |
| Funcionario → Venda | Obrigatório; exclusão restrita |
| Caixa → Venda | Obrigatório; exclusão restrita |
| Venda → ItemVenda | FK obrigatória; exclusão da venda em cascata |
| Produto → ItemVenda | FK obrigatória; exclusão do produto restrita |
| Venda → Pagamento | FK obrigatória; exclusão da venda em cascata |

Venda e Produto formam uma relação N:N por meio de ItemVenda, que tem ID próprio e guarda quantidade, preço, subtotal e desconto. O mesmo produto pode aparecer em mais de uma linha do cupom; por isso o par VendaId/ProdutoId não é único.

Índices únicos: número do caixa, nome da categoria, CPF e e-mail do cliente, CNPJ do fornecedor, CPF e matrícula do funcionário, código de barras e número do cupom. E-mails nulos são permitidos; MySQL aceita múltiplos NULLs em um índice único. As FKs também têm índices.

Uma FK garante que cada item aponta para uma venda existente; ela não exige que uma venda já nasça com pelo menos um item. Fechamento de venda, estoque, totais e conferência dos pagamentos são regras da futura camada de aplicação, fora do foco deste CP2.

O MER do CP1 foi preservado em [docs/mer.pdf](docs/mer.pdf). O esquema físico está em [docs/modelo-fisico.md](docs/modelo-fisico.md), e o SQL gerado pelo EF em [docs/schema.sql](docs/schema.sql).


## Verificar a persistência

Com o MySQL iniciado e a migration aplicada, execute:

```powershell
$env:ConnectionStrings__MySql = "Server=127.0.0.1;Port=3306;Database=Supermercado;User=root;Password=$env:MYSQL_ROOT_PASSWORD;"
dotnet run --project tests/Supermercado.PersistenceChecks
```

O programa verifica CRUD pelo repositório, persistência das nove entidades, valores monetários, múltiplos pagamentos, cliente opcional, e-mails nulos, unicidade, FKs, exclusão restrita, SET NULL e cascata. Os registros são criados dentro de uma transação revertida ao final. Use um banco local de desenvolvimento. Uma falha encerra o programa com erro.

A validação executada está registrada em [docs/validacao.md](docs/validacao.md).
