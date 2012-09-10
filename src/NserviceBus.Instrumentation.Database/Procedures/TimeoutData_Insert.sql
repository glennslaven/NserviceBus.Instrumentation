USE [NSBInstrumentation]
GO

if( object_id('[dbo].[TimeoutData_Insert]', 'P') is null) exec sp_executesql N'create procedure [dbo].[TimeoutData_Insert] as begin declare @a int; end'; 
GO 

ALTER PROCEDURE [dbo].[TimeoutData_Insert]
	@TimeoutDataId uniqueidentifier,
	@DocumentId [nvarchar](100),
	@SagaId [uniqueidentifier],
	@ServiceName [nvarchar](100),
	@MachineName [nvarchar](50),
	@ExpiresUtc datetime,
	@TimeoutState [nvarchar](max),
	@Data [nvarchar](max)
AS
	INSERT INTO [TimeoutData] (TimeoutDataId, [CreatedOnUtc],[DocumentId],[ServiceName],[MachineName],[SagaId],[ExpiresUtc],[TimeoutState],[TimeoutData])
	VALUES (@TimeoutDataId, GetUtcDate(), @DocumentId, @ServiceName, @MachineName, @SagaId, @ExpiresUtc, @TimeoutState, @Data);

GO