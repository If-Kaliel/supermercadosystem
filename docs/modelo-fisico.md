# Esquema físico — Supermercado

Modelo gerado pela única migration InitialCreate. As nove entidades e os GUIDs do CP1 foram mantidos; o CP2 explicita tipos, tamanhos, nullability, FKs, índices e comportamento de exclusão.

```mermaid
erDiagram
    Categorias ||--o{ Produtos : classifica
    Fornecedores ||--o{ Produtos : fornece
    Produtos ||--o{ ItensVenda : compoe
    Vendas ||--o{ ItensVenda : possui
    Clientes |o--o{ Vendas : identifica
    Funcionarios ||--o{ Vendas : opera
    Caixas ||--o{ Vendas : registra
    Vendas ||--o{ Pagamentos : recebe
    Categorias {
        char36 Id PK
        varchar100 Nome UK
        varchar500 Descricao "nullable"
        bool Ativo
    }
    Fornecedores {
        char36 Id PK
        varchar14 Cnpj UK
        varchar150 RazaoSocial
        varchar150 NomeFantasia
        varchar254 Email
        varchar20 Telefone "nullable"
        bool Ativo
    }
    Produtos {
        char36 Id PK
        char36 CategoriaId FK
        char36 FornecedorId FK
        varchar50 CodigoBarras UK
        varchar150 Nome
        decimal18_2 PrecoVenda
        decimal18_2 PrecoCusto
        int EstoqueAtual
        int EstoqueMinimo
    }
    Clientes {
        char36 Id PK
        varchar150 Nome
        varchar11 Cpf UK
        varchar254 Email UK "nullable"
        varchar20 Telefone "nullable"
        int PontosFidelidade
    }
    Funcionarios {
        char36 Id PK
        varchar150 Nome
        varchar11 Cpf UK
        varchar30 Matricula UK
        varchar80 Cargo
        datetime6 DataAdmissao
        bool Ativo
    }
    Caixas {
        char36 Id PK
        int NumeroCaixa UK
        varchar100 Localizacao
        varchar20 Status
    }
    Vendas {
        char36 Id PK
        char36 ClienteId FK "nullable"
        char36 FuncionarioId FK
        char36 CaixaId FK
        varchar50 NumeroCupom UK
        datetime6 DataHora
        decimal18_2 ValorTotal
        decimal18_2 DescontoTotal
        varchar20 Status
    }
    ItensVenda {
        char36 Id PK
        char36 VendaId FK
        char36 ProdutoId FK
        int Quantidade
        decimal18_2 PrecoUnitario
        decimal18_2 Subtotal
        decimal18_2 Desconto
    }
    Pagamentos {
        char36 Id PK
        char36 VendaId FK
        varchar30 FormaPagamento
        decimal18_2 Valor
        datetime6 DataHora
        varchar100 CodigoTransacao "nullable"
        varchar20 Status
    }
```

O diagrama mostra a cardinalidade permitida pelo banco. Vendas podem existir abertas sem itens/pagamentos; exigir itens para finalizar depende de regra de aplicação. ItemVenda materializa o N:N entre produtos e vendas. Não foram adicionadas entidades artificiais para criar 1:1 ou TPH, ausentes do domínio original.

A definição SQL exata, com nomes de constraints e índices, está em [schema.sql](schema.sql).
