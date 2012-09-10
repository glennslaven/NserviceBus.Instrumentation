USE [Instrumentation]
if( object_id('[dbo].[SagaData_Insert]', 'P') is null) exec sp_executesql N'create procedure [dbo].[SagaData_Insert] as begin declare @a int; end'; 
GO 

ALTER PROCEDURE [dbo].[SagaData_Insert]
	@SagaDataId uniqueidentifier,
	@SagaType [nvarchar](100),
	@SagaId [uniqueidentifier],
	@ServiceName [nvarchar](100),
	@MachineName [nvarchar](50),
	@Data [varchar](max)
AS
	INSERT INTO SagaData (SagaDataId, ServiceName, MachineName, SagaType, SagaId, SagaData, CreatedOnUtc) 
	VALUES (@SagaDataId, @ServiceName, @MachineName, @SagaType, @SagaId, @Data, GetUtcDate());
