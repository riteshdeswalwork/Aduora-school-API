IF OBJECT_ID(N'[AchievementTalents]', N'U') IS NULL
BEGIN
    CREATE TABLE [AchievementTalents] (
        [Id] uniqueidentifier NOT NULL,
        [AcademicYearId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NULL,
        [Title] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NULL,
        [Level] nvarchar(100) NULL,
        [AchievementDate] datetime2 NOT NULL,
        [Description] nvarchar(2000) NULL,
        [AwardedBy] nvarchar(150) NULL,
        [Type] nvarchar(30) NULL,
        [EventName] nvarchar(200) NULL,
        [Position] nvarchar(100) NULL,
        [CertificateIssued] bit NOT NULL,
        [Remarks] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AchievementTalents] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[OnlinePaymentTransactions]', N'U') IS NULL
BEGIN
    CREATE TABLE [OnlinePaymentTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [AcademicYearId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ConvenienceFees] decimal(18,2) NOT NULL,
        [TaxBreakdown] decimal(18,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL,
        [PaymentFor] nvarchar(100) NULL,
        [Gateway] nvarchar(100) NULL,
        [TransactionReference] nvarchar(150) NULL,
        [MerchantRefId] nvarchar(150) NULL,
        [RefundReference] nvarchar(150) NULL,
        [Status] nvarchar(30) NOT NULL,
        [SettlementStatus] nvarchar(30) NULL,
        [LatencyMs] int NOT NULL,
        [PaidAt] datetime2 NULL,
        [ReceiptNumber] nvarchar(100) NULL,
        [Notes] nvarchar(1000) NULL,
        [LedgerCategory] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OnlinePaymentTransactions] PRIMARY KEY ([Id])
    );
END
