CREATE TABLE dbo.Products
(
	ProductId int IDENTITY(1, 1) NOT NULL,
	Name nvarchar(100) NOT NULL,
	UnitPricePence int NOT NULL,
	CONSTRAINT PK_Products PRIMARY KEY (ProductId),
	CONSTRAINT UQ_Products_Name UNIQUE (Name),
	CONSTRAINT CK_Products_UnitPricePence CHECK (UnitPricePence >= 0)
);

CREATE TABLE dbo.ProductOffers
(
	ProductOfferId int IDENTITY(1, 1) NOT NULL,
	ProductId int NOT NULL,
	OfferType nvarchar(100) NOT NULL,
	ConfigurationVersion int NOT NULL CONSTRAINT DF_ProductOffers_ConfigurationVersion DEFAULT (1),
	ConfigurationJson nvarchar(max) NOT NULL,
	CONSTRAINT PK_ProductOffers PRIMARY KEY (ProductOfferId),
	CONSTRAINT FK_ProductOffers_Products FOREIGN KEY (ProductId)
		REFERENCES dbo.Products (ProductId) ON DELETE CASCADE,
	CONSTRAINT UQ_ProductOffers_ProductId_OfferType UNIQUE (ProductId, OfferType),
	CONSTRAINT CK_ProductOffers_OfferType CHECK (LEN(LTRIM(RTRIM(OfferType))) > 0),
	CONSTRAINT CK_ProductOffers_ConfigurationVersion CHECK (ConfigurationVersion > 0),
	CONSTRAINT CK_ProductOffers_ConfigurationJson CHECK (ISJSON(ConfigurationJson) = 1)
);
