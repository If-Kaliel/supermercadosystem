# SupermercadoSystem — CP2

Checkpoint 2 de .NET — 2TDSPB. Continuação do projeto de supermercado feito no CP1.

## Integrantes

| Nome | RM |
| --- | --- |
| Kaliel Aquino | 567587 |
| Andre Matuda | 566733 |
| Paulo Diedrich | 567618 |

Os integrantes acima são do CP2. O PDF do MER foi preservado como registro do CP1 e contém a composição do grupo daquela etapa.

## Domínio

Sistema de gestão de supermercado e frente de caixa: produtos, categorias, fornecedores, clientes, funcionários, caixas, vendas, itens de venda e pagamentos.

O professor autorizou a continuidade do domínio Supermercado. Por isso, usamos o MER do nosso CP1 no lugar do Recommenda apresentado no enunciado. Nosso modelo tem relações 1:N e N:N por ItemVenda, além de cliente opcional na venda. Não há relação 1:1 nem herança TPH no MER deste domínio.

## O que foi feito no CP2

- Persistência das nove entidades com Entity Framework Core e MySQL.
- DbContext e configurações Fluent API na Infrastructure.
- Chaves primárias e estrangeiras, campos obrigatórios, tamanhos e índices.
- Repositório genérico com interface na Application e implementação na Infrastructure.
- Contexto e repositórios com ciclo de vida scoped, registrados no Program.cs da API.
- Migrations versionadas e endpoints de categorias para demonstrar o acesso ao banco.

## Organização da solução

| Projeto | Responsabilidade |
| --- | --- |
| Supermercado.Domain | Entidades do CP1, sem dependência do EF Core |
| Supermercado.Application | Interface IRepository<T> |
| Supermercado.Infrastructure | Contexto, mapeamentos, repositório e migrations |
| Supermercado.Api | Endpoints, configuração e injeção de dependência |
| Supermercado.PersistenceChecks | Verificação de integração com MySQL |

O repositório usa a mesma estratégia para todas as entidades. Os métodos de inclusão, alteração e remoção precisam de `SaveChangesAsync` para gravar no banco. Não foram adicionadas regras de fechamento de caixa, estoque ou cálculo de venda na Infrastructure.

## Requisitos

- SDK .NET 10 e PowerShell 5.1 ou 7.
- Docker Desktop com engine Linux em execução.
- Porta 3306 disponível para o MySQL.

Pacotes usados: EF Core e dotnet-ef 10.0.11, MySql.EntityFrameworkCore 10.0.9 e Microsoft.AspNetCore.OpenApi 10.0.11.

## Banco de dados e preparação

O SGBD é **MySQL em Docker**. Neste ambiente, usamos o container `TDSPB`, porta `3306`, banco `Supermercado` e MySQL 26.7.0. O container e o volume existentes foram preservados.

Com o Docker Desktop iniciado e a API parada no Rider, execute uma vez na raiz do projeto:

```powershell
.\scripts\Iniciar-Desenvolvimento.ps1
```

O script seleciona o SDK .NET 10, inicia ou reaproveita o MySQL, restaura pacotes e ferramentas, compila e aplica as migrations. Ele verifica se a API está em execução antes de recompilar, para evitar arquivos bloqueados no Windows. A conexão é salva em `src/Supermercado.Api/appsettings.Local.json`, ignorado pelo Git. Não é necessário copiar ou digitar a senha em cada execução.

Na primeira execução, uma conexão válida que já esteja em User Secrets é transferida para o arquivo local. A chave antiga só é removida após validar o acesso com EF Core, mantendo os demais segredos. Se não houver configuração local, o script usa a credencial do container existente; se o volume foi inicializado com outra senha, a validação falha e os dados são preservados.

Em uma máquina sem `TDSPB`, o script cria o container com senha aleatória, volume nomeado `TDSPB-dados`, porta publicada somente em localhost e a imagem MySQL fixada por digest. O digest corresponde à versão validada neste projeto. Containers e volumes existentes nunca são removidos. A política `unless-stopped` mantém o serviço disponível após reiniciar o Docker, salvo quando ele foi parado manualmente.

Para executar também as verificações de persistência e a consulta HTTP real:

```powershell
.\scripts\Iniciar-Desenvolvimento.ps1 -Verificar
```

O projeto de verificações é um programa de integração, executado com `dotnet run`; não é uma suíte xUnit/MSTest. O script também executa `dotnet test`, mas as 16 verificações reais são feitas pelo programa de integração. Seus registros são revertidos em transação. A API temporária usada para verificar HTTP é encerrada ao final.

## Configuração da conexão

`appsettings.json` e `appsettings.Development.json` contêm configurações compartilhadas, sem credenciais. A conexão de desenvolvimento fica no arquivo local. Há um exemplo sem credenciais reais em `appsettings.Local.example.json`.

Em Development, a precedência da conexão, da maior para a menor, é: argumentos de linha de comando, variáveis de ambiente, arquivo local, User Secrets e arquivos appsettings padrão. Valores vazios são desconsiderados. O log inicial informa a origem efetiva, servidor, porta e banco, sem imprimir a senha.

Em outros ambientes, configure `ConnectionStrings__MySql` externamente. O arquivo local só é carregado em Development e não é incluído na publicação. Uma conexão ausente gera uma mensagem de configuração na inicialização.

O script detecta uma variável `ConnectionStrings__MySql` conflitante e interrompe antes de aplicar migrations. Também confere servidor, porta e banco para evitar atualizar um destino diferente por engano.

## Aplicar as migrations manualmente

O script já executa essa etapa. Para repetir separadamente após preparar o ambiente:

```powershell
dotnet ef database update --project src/Supermercado.Infrastructure --startup-project src/Supermercado.Api -- --environment Development
```

São **duas migrations**, dentro do limite do CP2:

1. `InitialCreate`: cria as nove tabelas, chaves e índices.
2. `AjustaTiposConformeMer`: corrige os limites de texto e a precisão monetária para corresponder ao dicionário de dados do CP1. A primeira migration já havia sido aplicada, por isso o ajuste foi feito em uma segunda migration, preservando o histórico.

A segunda migration ajusta Categoria.Nome para 80, Categoria.Descricao para 255, Fornecedor.Email para 100, Produto.Nome para 120, Venda.NumeroCupom para 30 e os valores monetários para `decimal(10,2)`. Os demais tamanhos não especificados no CP1 foram definidos no mapeamento. Dados anteriores precisam respeitar esses limites para a atualização ser aplicada.

Para conferir se o modelo está sincronizado com as migrations:

```powershell
dotnet ef migrations has-pending-model-changes --project src/Supermercado.Infrastructure --startup-project src/Supermercado.Api -- --environment Development
```

## Executar a API

No Rider, abra `Supermercado.slnx`, selecione **Supermercado.Api: http** ou **https** e clique em Run. Os dois perfis definem `DOTNET_ENVIRONMENT` e `ASPNETCORE_ENVIRONMENT` como `Development`. A conexão local é carregada automaticamente; não informe senha nos argumentos da IDE.

Pelo terminal, o script também pode preparar o banco e manter a API em execução:

```powershell
.\scripts\Iniciar-Desenvolvimento.ps1 -ExecutarApi
```

Ou, com o ambiente já preparado:

```powershell
dotnet run --project src/Supermercado.Api --launch-profile http
```

Em outro PowerShell:

```powershell
Invoke-RestMethod http://localhost:5278/categorias
Invoke-RestMethod http://localhost:5278/categorias -Method Post -ContentType "application/json" -Body '{"nome":"Mercearia","descricao":"Alimentos e itens de despensa"}'
```

| Endpoint | Resultado |
| --- | --- |
| GET /categorias | Lista as categorias |
| GET /categorias/{id} | Busca uma categoria; retorna 404 se não existir |
| POST /categorias | Cadastra uma categoria; retorna 201, 400 para dados inválidos ou 409 para nome duplicado |
| GET /openapi/v1.json | Documento OpenAPI em desenvolvimento |

O nome da categoria aceita até 80 caracteres e a descrição opcional até 255. Os exemplos também estão em `src/Supermercado.Api/Supermercado.Api.http`, que pode ser executado no Rider.

## Mapeamento

Todas as entidades têm PK do tipo Guid, gerada pela aplicação e armazenada como `char(36)`. Os valores monetários usam `decimal(10,2)`, conforme o MER.

| Relacionamento | Regra no banco |
| --- | --- |
| Categoria → Produto | Categoria obrigatória; exclusão restrita |
| Fornecedor → Produto | Fornecedor obrigatório; exclusão restrita |
| Cliente → Venda | Cliente opcional; exclusão deixa ClienteId nulo |
| Funcionario → Venda | Funcionário obrigatório; exclusão restrita |
| Caixa → Venda | Caixa obrigatório; exclusão restrita |
| Produto → ItemVenda | Produto obrigatório; exclusão restrita |
| Venda → ItemVenda | Venda obrigatória; exclusão em cascata |
| Venda → Pagamento | Venda obrigatória; exclusão em cascata |

ItemVenda representa o N:N entre Produto e Venda e guarda quantidade, preço, subtotal e desconto. Os índices únicos incluem CPF, CNPJ, matrícula, código de barras, número do caixa, número do cupom, nome da categoria e e-mail do cliente. O e-mail do cliente pode ser nulo.

No CP1, uma categoria deve ter produtos, uma venda válida deve ter itens e uma venda concluída deve ter pagamento. As FKs garantem os vínculos dos filhos com os pais; esses mínimos de filhos dependem de validação na aplicação ao concluir a operação. O CP2 fica na persistência, sem implementar essas regras de negócio.

## Verificação da persistência

Com o banco iniciado e migrado:

```powershell
.\scripts\Iniciar-Desenvolvimento.ps1 -Verificar
```

O programa verifica CRUD, FKs, unicidade, cliente opcional, valores monetários, limites de texto e exclusões. Os dados são criados em uma transação revertida ao final.

## Documentação

- [MER do CP1](docs/mer.pdf)
- [Modelo físico](docs/modelo-fisico.md)
- [SQL das migrations](docs/schema.sql)
- [Resultados da validação](docs/validacao.md)
- [Diagnóstico e correção da infraestrutura](docs/infraestrutura.md)

A entrega no portal é somente o link do repositório do GitHub.
