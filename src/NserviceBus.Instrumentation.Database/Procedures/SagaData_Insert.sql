USE [NSBInstrumentation]
GO

if( object_id('[dbo].[SagaData_Insert]', 'P') is null) exec sp_executesql N'create procedure [dbo].[SagaData_Insert] as begin declare @a int; end'; 
GO 

ALTER PROCEDURE [dbo].[SagaData_Insert]
	@SagaType [nvarchar](100),
	@SagaId [uniqueidentifier],
	@ServiceName [nvarchar](100),
	@MachineName [nvarchar](50),
	@Data [text]
AS
	INSERT INTO SagaData (ServiceName, MachineName, SagaType, SagaId, SagaData, CreatedOnUtc) 
	VALUES (@ServiceName, @MachineName, @SagaType, @SagaId, @Data, GetUtcDate());

GO