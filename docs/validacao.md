# Validação do CP2

Revisão executada em 08/10/2026, na branch main. A correção posterior da configuração local está registrada em [Infraestrutura](infraestrutura.md).

## Ambiente

- SDK .NET 10.0.400.
- EF Core e dotnet-ef 10.0.11.
- MySql.EntityFrameworkCore 10.0.9.
- MySQL Community Server 26.7.0, container TDSPB na porta 3306.
- Credencial inicialmente em User Secrets, depois consolidada no arquivo local ignorado pelo Git; sem senha versionada.

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

## Revisão de qualidade

A revisão foi repetida em uma cópia limpa da main baixada do GitHub, no commit `7eb133d`. A validação usou um banco separado, `SupermercadoReview_20261008`, sem alterar os dados do banco do projeto.

| Item revisado | Evidência |
| --- | --- |
| Reprodução da entrega | Restore, tool restore e build Release concluídos, sem erros ou avisos |
| Banco novo | As duas migrations aplicadas e as 16 verificações de persistência aprovadas |
| SQL entregue | Script regenerado corresponde a docs/schema.sql |
| Esquema físico | Nove tabelas de entidades, tipos, nullability, índices e oito FKs conferidos no MySQL |
| Dependências | NuGet não reportou vulnerabilidades conhecidas, incluindo dependências transitivas |
| API | Casos abaixo responderam conforme esperado; documento OpenAPI acessível |
| Limpeza | API temporária encerrada e banco de revisão removido |

Além dos cenários anteriores, foram conferidos corpo JSON nulo, JSON incompleto, tipo incorreto para o nome e nome ausente (400), tipo de mídia incompatível (415), cadastro nos limites de 80/255 caracteres (201), leitura do conteúdo persistido (200) e tentativa de cadastro duplicado (409).

Não foram encontrados problemas técnicos impeditivos no escopo de persistência. Foi esclarecida no README a diferença entre os integrantes atuais do CP2 e o grupo registrado no PDF histórico do CP1. O domínio Supermercado segue a autorização do professor informada pelo grupo; o MER usado não contém relação 1:1 nem herança TPH. Regras de estoque, fechamento de venda e mínimos de filhos continuam fora desta etapa, conforme explicado no README.
