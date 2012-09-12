USE [Instrumentation]
if( object_id('[dbo].[Web_ServiceController_Detail]', 'P') is null) exec sp_executesql N'create procedure [dbo].[Web_ServiceController_Detail] as begin declare @a int; end'; 
GO 

ALTER PROCEDURE [dbo].[Web_ServiceController_Detail]	
	@ServiceName [nvarchar](100),
	@MachineName [nvarchar](50)
AS
	select s.SagaDataId, s.SagaId
	INTO #SagaDataIds
	FROM
	(
		select SagaDataId, SagaId, ROW_NUMBER() OVER (PARTITION BY SagaType, SagaId ORDER BY CreatedOnUtc DESC) as RecordIndex
		from sagadata
		WHERE MachineName = @MachineName AND ServiceName = @ServiceName
	) as s	
	where RecordIndex = 1;
	
	SELECT s.SagaDataId, s.SagaId, SagaType, ServiceName, MachineName, sv.SagaDataId, KeyName 'Key', KeyValue 'Value'
	FROM SagaData s
	JOIN SagaDataValues sv ON (s.SagaDataId = sv.SagaDataId)
	JOIN #SagaDataIds sid ON (s.SagaDataId = sid.SagaDataId);
	
	SELECT t.TimeoutDataId, t.SagaId, t.ExpiresUtc, td.TimeoutDataId, td.KeyName 'Key', td.KeyValue 'Value'
	FROM
	(
		SELECT TimeoutDataId, td.SagaId, ExpiresUtc, ROW_NUMBER() OVER (PARTITION BY DocumentId ORDER BY CreatedOnUtc DESC) as RecordIndex
		FROM TimeoutData td
		JOIN #SagaDataIds sid ON (td.SagaId = sid.SagaId)
		WHERE MachineName = @MachineName AND ServiceName = @ServiceName
	) as t
	JOIN TimeoutDataValues td ON (t.TimeoutDataId = td.TimeoutDataId)
	WHERE t.RecordIndex = 1;
	
