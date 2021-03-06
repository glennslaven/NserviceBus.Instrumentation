﻿USE [Instrumentation]
IF OBJECT_ID('SagaData') IS NULL BEGIN

	CREATE TABLE [dbo].[SagaData](
		[SagaDataId] [uniqueidentifier] NOT NULL,
		[SagaType] [nvarchar](100) NOT NULL,
		[SagaId] [uniqueidentifier] NOT NULL,
		[CreatedOnUtc] [datetime] NOT NULL,
		[ServiceName] [nvarchar](100) NOT NULL,
		[MachineName] [nvarchar](50) NOT NULL,
		[SagaData] [varchar](max) NOT NULL,
		[Archive] [bit] NOT NULL
	 CONSTRAINT [PK_SagaData] PRIMARY KEY CLUSTERED 
	(
		[SagaDataId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

	ALTER TABLE [dbo].[SagaData] ADD  CONSTRAINT [DF_SagaData_SagaDataid]  DEFAULT (newid()) FOR [SagaDataId];
END;