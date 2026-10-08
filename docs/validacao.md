# Validação do CP2

Executada em 07/10/2026 no ambiente local:

- SDK .NET 10.0.400.
- EF Core e dotnet-ef 10.0.11; MySql.EntityFrameworkCore 10.0.9.
- MySQL Community Server 26.7.0, imagem mysql:latest, container TDSPB, porta 3306.
- Senha local armazenada em User Secrets; nenhum segredo versionado.

## Resultados

| Verificação | Resultado |
| --- | --- |
| Build da solução | Sucesso, zero erros e avisos |
| Migration InitialCreate em banco novo | Aplicada com sucesso |
| Segunda execução de database update | Banco atualizado; nenhuma migration reaplicada |
| Modelo comparado com a migration | Sem alterações pendentes |
| Tabelas no MySQL | Nove entidades e __EFMigrationsHistory |
| Verificação de integração | 12 verificações passaram |
| GET /categorias | 200 |
| POST /categorias | 201 |
| GET /categorias/{id} existente | 200 |
| POST com nome duplicado | 409 |
| POST com nome vazio | 400 |
| GET com ID inexistente | 404 |
| GET /openapi/v1.json | 200 |

A verificação de integração persiste todas as nove entidades e testa CRUD do repositório, cliente opcional, múltiplos e-mails nulos, precisão monetária, múltiplos pagamentos, índice único, FK inválida, exclusão restrita de produto vendido, SET NULL na exclusão do cliente e cascata dos itens/pagamentos. A transação é revertida ao final.

## Observação de ambiente

O Docker Desktop apresentou erro de socket órfão em sailor-ingest.sock. O engine voltou a funcionar após guardar como backup e recriar somente as pastas temporárias de sockets, com os processos encerrados. Esse é um [problema relatado no repositório do Docker Desktop](https://github.com/docker/desktop-feedback/issues/554). As pastas de dados de containers e volumes não foram alteradas nessa recuperação.

A configuração do Rider deve usar o SDK .NET 10. A instalação antiga em Program Files contém .NET 5; neste computador o SDK 10 está no diretório .dotnet do usuário.

