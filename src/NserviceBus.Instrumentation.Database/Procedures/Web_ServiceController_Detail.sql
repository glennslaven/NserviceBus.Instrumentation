USE [Instrumentation]
if( object_id('[dbo].[Web_ServiceController_Detail]', 'P') is null) exec sp_executesql N'create procedure [dbo].[Web_ServiceController_Detail] as begin declare @a int; end'; 
GO 

ALTER PROCEDURE [dbo].[Web_ServiceController_Detail]	
	@ServiceName [nvarchar](100),
	@MachineName [nvarchar](50)
AS
	select s.SagaDataId, SagaId, SagaType, ServiceName, MachineName, sv.SagaDataId, KeyName, KeyValue
	FROM
	(
		select SagaDataId, SagaId, SagaType, ServiceName, MachineName, ROW_NUMBER() OVER (PARTITION BY SagaType, SagaId ORDER BY CreatedOnUtc DESC) as RecordIndex
		from sagadata
		WHERE MachineName = @MachineName AND ServiceName = @ServiceName
	) as s
	JOIN SagaDataValues sv ON (s.SagaDataId = sv.SagaDataId)
	where RecordIndex = 1
	
