# Validação do CP2

Revisão final executada em 08/10/2026, na branch main.

## Ambiente

- SDK .NET 10.0.400.
- EF Core e dotnet-ef 10.0.11.
- MySql.EntityFrameworkCore 10.0.9.
- MySQL Community Server 26.7.0, container TDSPB na porta 3306.
- Credencial local em User Secrets, sem senha versionada.

## Persistência e migrations

| Verificação | Resultado |
| --- | --- |
| Build da solução | Zero erros e avisos |
| Atualização do banco que já tinha InitialCreate | AjustaTiposConformeMer aplicada |
| Aplicação em banco novo | As duas migrations aplicadas com sucesso |
| Nova execução de database update | Sucesso, sem mudanças adicionais |
| Comparação do modelo com o snapshot | Sem alterações pendentes |
| Integração no banco existente e no banco novo | 16 verificações passaram em cada cenário |
| Dados de teste | Transações revertidas, registros HTTP e banco temporário removidos |

As verificações cobrem inclusão, busca, atualização e remoção pelo repositório; persistência das nove entidades; venda sem cliente; e-mails nulos; precisão monetária; múltiplos pagamentos; índice único; FK inválida; cliente informado inexistente; limites de 80/255 caracteres; exclusão restrita; SET NULL e cascata.

Os limites do mapeamento foram conferidos com o dicionário de dados do CP1. A segunda migration está justificada no README e mantém o total de migrations dentro do limite do enunciado.

## API

| Cenário | HTTP |
| --- | --- |
| Listar categorias | 200 |
| Cadastrar categoria | 201 |
| Buscar categoria existente | 200 |
| Nome duplicado | 409 |
| Nome vazio | 400 |
| Nome com 81 caracteres | 400 |
| Descrição com 256 caracteres | 400 |
| Nome com 80 e descrição com 255 caracteres | 201 |
| ID inexistente | 404 |
| Documento OpenAPI em desenvolvimento | 200 |

## Organização

DbContext, configurações e repositório concreto ficam na Infrastructure. O contrato fica na Application e o registro scoped fica no Program.cs da API. O Domain continua sem dependência de EF Core. Não há bin/obj versionados.
