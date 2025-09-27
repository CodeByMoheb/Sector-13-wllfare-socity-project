-- Create LeadershipMessages table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LeadershipMessages' AND xtype='U')
BEGIN
    CREATE TABLE [LeadershipMessages] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [Message] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(500) NULL,
        [MessageType] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LeadershipMessages] PRIMARY KEY ([Id])
    );
END

-- Create ElectedCandidates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ElectedCandidates' AND xtype='U')
BEGIN
    CREATE TABLE [ElectedCandidates] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [IsPresident] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [ElectionYear] nvarchar(20) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ElectedCandidates] PRIMARY KEY ([Id])
    );
END

-- Create PreviousCandidates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PreviousCandidates' AND xtype='U')
BEGIN
    CREATE TABLE [PreviousCandidates] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [IsPresident] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [TermPeriod] nvarchar(20) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PreviousCandidates] PRIMARY KEY ([Id])
    );
END
