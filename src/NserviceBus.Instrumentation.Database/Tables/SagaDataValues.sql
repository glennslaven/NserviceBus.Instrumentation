USE [NSBInstrumentation]
GO

IF OBJECT_ID('SagaDataValues') IS NULL BEGIN

	CREATE TABLE [dbo].[SagaDataValues](
		[SagaDataId] [uniqueidentifier] NOT NULL,
		[KeyName] [nvarchar](50) NOT NULL,
		[KeyValue] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_SagaDataValues] PRIMARY KEY CLUSTERED 
	(
		[SagaDataId] ASC,
		[KeyName] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY];

END;