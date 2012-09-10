USE [NSBInstrumentation]
GO

IF OBJECT_ID('TimeoutData') IS NULL BEGIN

	CREATE TABLE [dbo].[TimeoutData](
		[TimeoutDataId] [uniqueidentifier] NOT NULL,
		[CreatedOnUtc] [datetime] NOT NULL,
		[DocumentId] [nvarchar](100) NOT NULL,
		[ServiceName] [nvarchar](100) NOT NULL,
		[MachineName] [nvarchar](50) NOT NULL,
		[SagaId] [uniqueidentifier] NOT NULL,
		[ExpiresUtc] [datetime] NOT NULL,
		[TimeoutState] [nvarchar](max) NOT NULL,
		[TimeoutData] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_TimeoutData] PRIMARY KEY CLUSTERED 
	(
		[TimeoutDataId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

	ALTER TABLE [dbo].[TimeoutData] ADD  CONSTRAINT [DF_TimeoutData_TimeoutDataId]  DEFAULT (newid()) FOR [TimeoutDataId];

END;