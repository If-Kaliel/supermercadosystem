CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `Caixas` (
    `Id` char(36) NOT NULL,
    `NumeroCaixa` int NOT NULL,
    `Localizacao` varchar(100) NOT NULL,
    `Status` varchar(20) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Categorias` (
    `Id` char(36) NOT NULL,
    `Nome` varchar(100) NOT NULL,
    `Descricao` varchar(500) NULL,
    `Ativo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Clientes` (
    `Id` char(36) NOT NULL,
    `Nome` varchar(150) NOT NULL,
    `Cpf` varchar(11) NOT NULL,
    `Email` varchar(254) NULL,
    `Telefone` varchar(20) NULL,
    `PontosFidelidade` int NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Fornecedores` (
    `Id` char(36) NOT NULL,
    `RazaoSocial` varchar(150) NOT NULL,
    `NomeFantasia` varchar(150) NOT NULL,
    `Cnpj` varchar(14) NOT NULL,
    `Email` varchar(254) NOT NULL,
    `Telefone` varchar(20) NULL,
    `Ativo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Funcionarios` (
    `Id` char(36) NOT NULL,
    `Nome` varchar(150) NOT NULL,
    `Cpf` varchar(11) NOT NULL,
    `Matricula` varchar(30) NOT NULL,
    `Cargo` varchar(80) NOT NULL,
    `DataAdmissao` datetime(6) NOT NULL,
    `Ativo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Produtos` (
    `Id` char(36) NOT NULL,
    `CategoriaId` char(36) NOT NULL,
    `FornecedorId` char(36) NOT NULL,
    `CodigoBarras` varchar(50) NOT NULL,
    `Nome` varchar(150) NOT NULL,
    `PrecoVenda` decimal(18,2) NOT NULL,
    `PrecoCusto` decimal(18,2) NOT NULL,
    `EstoqueAtual` int NOT NULL,
    `EstoqueMinimo` int NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Produtos_Categorias_CategoriaId` FOREIGN KEY (`CategoriaId`) REFERENCES `Categorias` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Produtos_Fornecedores_FornecedorId` FOREIGN KEY (`FornecedorId`) REFERENCES `Fornecedores` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Vendas` (
    `Id` char(36) NOT NULL,
    `ClienteId` char(36) NULL,
    `FuncionarioId` char(36) NOT NULL,
    `CaixaId` char(36) NOT NULL,
    `NumeroCupom` varchar(50) NOT NULL,
    `DataHora` datetime(6) NOT NULL,
    `ValorTotal` decimal(18,2) NOT NULL,
    `DescontoTotal` decimal(18,2) NOT NULL,
    `Status` varchar(20) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Vendas_Caixas_CaixaId` FOREIGN KEY (`CaixaId`) REFERENCES `Caixas` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Vendas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Vendas_Funcionarios_FuncionarioId` FOREIGN KEY (`FuncionarioId`) REFERENCES `Funcionarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `ItensVenda` (
    `Id` char(36) NOT NULL,
    `VendaId` char(36) NOT NULL,
    `ProdutoId` char(36) NOT NULL,
    `Quantidade` int NOT NULL,
    `PrecoUnitario` decimal(18,2) NOT NULL,
    `Subtotal` decimal(18,2) NOT NULL,
    `Desconto` decimal(18,2) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ItensVenda_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ItensVenda_Vendas_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `Vendas` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Pagamentos` (
    `Id` char(36) NOT NULL,
    `VendaId` char(36) NOT NULL,
    `FormaPagamento` varchar(30) NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `DataHora` datetime(6) NOT NULL,
    `CodigoTransacao` varchar(100) NULL,
    `Status` varchar(20) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pagamentos_Vendas_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `Vendas` (`Id`) ON DELETE CASCADE
);

CREATE UNIQUE INDEX `IX_Caixas_NumeroCaixa` ON `Caixas` (`NumeroCaixa`);

CREATE UNIQUE INDEX `IX_Categorias_Nome` ON `Categorias` (`Nome`);

CREATE UNIQUE INDEX `IX_Clientes_Cpf` ON `Clientes` (`Cpf`);

CREATE UNIQUE INDEX `IX_Clientes_Email` ON `Clientes` (`Email`);

CREATE UNIQUE INDEX `IX_Fornecedores_Cnpj` ON `Fornecedores` (`Cnpj`);

CREATE UNIQUE INDEX `IX_Funcionarios_Cpf` ON `Funcionarios` (`Cpf`);

CREATE UNIQUE INDEX `IX_Funcionarios_Matricula` ON `Funcionarios` (`Matricula`);

CREATE INDEX `IX_ItensVenda_ProdutoId` ON `ItensVenda` (`ProdutoId`);

CREATE INDEX `IX_ItensVenda_VendaId` ON `ItensVenda` (`VendaId`);

CREATE INDEX `IX_Pagamentos_VendaId` ON `Pagamentos` (`VendaId`);

CREATE INDEX `IX_Produtos_CategoriaId` ON `Produtos` (`CategoriaId`);

CREATE UNIQUE INDEX `IX_Produtos_CodigoBarras` ON `Produtos` (`CodigoBarras`);

CREATE INDEX `IX_Produtos_FornecedorId` ON `Produtos` (`FornecedorId`);

CREATE INDEX `IX_Vendas_CaixaId` ON `Vendas` (`CaixaId`);

CREATE INDEX `IX_Vendas_ClienteId` ON `Vendas` (`ClienteId`);

CREATE INDEX `IX_Vendas_FuncionarioId` ON `Vendas` (`FuncionarioId`);

CREATE UNIQUE INDEX `IX_Vendas_NumeroCupom` ON `Vendas` (`NumeroCupom`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261008020101_InitialCreate', '10.0.11');

ALTER TABLE `Vendas` MODIFY `ValorTotal` decimal(10,2) NOT NULL;

ALTER TABLE `Vendas` MODIFY `NumeroCupom` varchar(30) NOT NULL;

ALTER TABLE `Vendas` MODIFY `DescontoTotal` decimal(10,2) NOT NULL;

ALTER TABLE `Produtos` MODIFY `PrecoVenda` decimal(10,2) NOT NULL;

ALTER TABLE `Produtos` MODIFY `PrecoCusto` decimal(10,2) NOT NULL;

ALTER TABLE `Produtos` MODIFY `Nome` varchar(120) NOT NULL;

ALTER TABLE `Pagamentos` MODIFY `Valor` decimal(10,2) NOT NULL;

ALTER TABLE `ItensVenda` MODIFY `Subtotal` decimal(10,2) NOT NULL;

ALTER TABLE `ItensVenda` MODIFY `PrecoUnitario` decimal(10,2) NOT NULL;

ALTER TABLE `ItensVenda` MODIFY `Desconto` decimal(10,2) NOT NULL;

ALTER TABLE `Fornecedores` MODIFY `Email` varchar(100) NOT NULL;

ALTER TABLE `Categorias` MODIFY `Nome` varchar(80) NOT NULL;

ALTER TABLE `Categorias` MODIFY `Descricao` varchar(255) NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261008214302_AjustaTiposConformeMer', '10.0.11');

COMMIT;

