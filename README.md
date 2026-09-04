# Checkpoint 1 — 2TDSPB (2026 segundo semestre)
## Modelo Entidade-Relacionamento (MER) e Arquitetura WebAPI

### 👥 Integrantes do Grupo
* **Nome:** Kaliel Aquino - **RM:** 567587
* **Nome:** Andre Matuda - **RM:** 566733
* **Nome:** Guilherme Anitelli - **RM:** 566744

---

### 🛒 Domínio Escolhido
**Sistema de Gestão de Supermercado e Frente de Caixa (PDV)**  
O domínio cobre desde a catalogação de mercadorias e gestão de fornecedores até a execução de vendas nos caixas (checkout), fidelização de clientes e liquidação de pagamentos em múltiplos meios.

---

### 📦 Entidades Modeladas (9 Entidades)
1. **Categoria:** Classificação e seções mercadológicas de produtos.
2. **Fornecedor:** Cadastro de distribuidores e fabricantes homologados.
3. **Produto:** Itens comercializáveis com controle de preço de custo/venda e estoque.
4. **Cliente:** Consumidores cadastrados no clube de benefícios e pontuação.
5. **Funcionario:** Operadores de caixa e colaboradores responsáveis pela venda.
6. **Caixa:** Identificação do terminal físico de PDV no estabelecimento.
7. **Venda:** Registro mestre do cupom fiscal e status da transação.
8. **ItemVenda:** Tabela associativa com histórico congelado do preço de venda e quantidade.
9. **Pagamento:** Conciliação financeira e múltiplos meios de pagamento da transação.

---

### 🔗 Resumo dos Relacionamentos
* **Categoria (1:N) Produto:** Uma categoria classifica um ou mais produtos (Obrigatório).
* **Fornecedor (1:N) Produto:** Um fornecedor fornece zero ou muitos produtos cadastrados.
* **Produto (1:N) ItemVenda:** Um produto pode compor múltiplos itens de venda ao longo do tempo.
* **Venda (1:N) ItemVenda:** Uma venda deve possuir no mínimo 1 item para ser válida (Obrigatório).
* **Cliente (0..1:N) Venda:** **Opcional** — O consumidor não é obrigado a informar o CPF no checkout.
* **Funcionario (1:N) Venda:** Uma venda é operada por exatamente um funcionário (Obrigatório).
* **Caixa (1:N) Venda:** Uma venda é realizada em um terminal físico (Obrigatório).
* **Venda (1:N) Pagamento:** Uma venda pode ser quitada com um ou múltiplos pagamentos (ex.: Dinheiro + PIX).

---

### 🔑 Estratégia de Identificação
Foi adotado o **`Guid` (UUID v4)** como chave primária (PK) em todas as entidades. Esta decisão técnica viabiliza a operação de frentes de caixa em contingência (offline), permitindo a geração de vendas e cupons de forma descentralizada sem qualquer risco de colisão de chaves com o servidor central.

---

### 📁 Diagrama do Modelo Relacional
O diagrama visual encontra-se disponível no caminho:
`docs/mer.png` (e em formato vetorial com documentação em `docs/mer.pdf`).