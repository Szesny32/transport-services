﻿CREATE TABLE [dbo].[Users]
(
	[id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[login] NVARCHAR(30),
	[sha256] NVARCHAR(64),
	[salt] NVARCHAR(44),
	[type] INT NOT NULL
)
