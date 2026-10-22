/* Read-only diagnostic. Run against the SQL Server used by the test station.
   No schema changes, result updates or measurement value substitutions. */
USE [OCV_DATA];
GO
SET NOCOUNT ON;

SELECT CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)) AS ServerName,
       DB_NAME() AS DatabaseName;

-- Actual precision of the TVP which receives @Results.
SELECT N'dbo.TestResultInputType' AS ObjectName,
       c.column_id AS ColumnOrder, c.name AS FieldName,
       TYPE_NAME(c.system_type_id) AS DataType,
       c.precision AS TotalDigits, c.scale AS FractionalDigits,
       c.precision-c.scale AS IntegerDigits, c.is_nullable AS IsNullable
FROM sys.table_types t
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
INNER JOIN sys.columns c ON c.object_id=t.type_table_object_id
WHERE s.name=N'dbo' AND t.name=N'TestResultInputType'
ORDER BY c.column_id;

-- Destination table, for comparison with the TVP.
SELECT N'dbo.TestResult' AS ObjectName,
       c.column_id AS ColumnOrder, c.name AS FieldName,
       TYPE_NAME(c.system_type_id) AS DataType,
       c.precision AS TotalDigits, c.scale AS FractionalDigits,
       c.precision-c.scale AS IntegerDigits, c.is_nullable AS IsNullable
FROM sys.columns c
WHERE c.object_id=OBJECT_ID(N'dbo.TestResult')
  AND c.system_type_id IN (106,108)
ORDER BY c.column_id;

-- Check the actual stored procedure parameter type.
SELECT p.name AS ParameterName,
       SCHEMA_NAME(t.schema_id)+N'.'+t.name AS ParameterType,
       p.is_readonly AS IsReadOnly
FROM sys.parameters p
INNER JOIN sys.types t ON t.user_type_id=p.user_type_id
WHERE p.object_id=OBJECT_ID(N'dbo.usp_SaveTestResults')
ORDER BY p.parameter_id;

SELECT TOP (10) TestTaskId, Status, CreatedAt, StartTime, EndTime, ErrorMessage
FROM dbo.TestTask
WHERE Status=N'Failed'
ORDER BY TestTaskId DESC;
