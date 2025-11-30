-- SQL Server Table Size Analysis Script
-- This script shows the space used by each table in your database
-- including data size, index size, and total size

SELECT
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows AS RowCounts,
    CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
    CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS UsedSpaceMB,
    CAST(ROUND(((SUM(a.total_pages) - SUM(a.used_pages)) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS UnusedSpaceMB,
    CAST(ROUND(((SUM(a.data_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS DataSpaceMB,
    CAST(ROUND((((SUM(a.used_pages) - SUM(a.data_pages)) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS IndexSpaceMB
FROM
    sys.tables t
INNER JOIN
    sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN
    sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN
    sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN
    sys.schemas s ON t.schema_id = s.schema_id
WHERE
    t.NAME NOT LIKE 'dt%'
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255
GROUP BY
    t.Name, s.Name, p.Rows
ORDER BY
    TotalSpaceMB DESC;

-- Additional query: Show percentage of total database size
WITH TableSizes AS (
    SELECT
        t.NAME AS TableName,
        CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB
    FROM
        sys.tables t
    INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
    INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
    INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
    WHERE
        t.is_ms_shipped = 0
    GROUP BY
        t.Name
)
SELECT
    TableName,
    TotalSpaceMB,
    CAST(ROUND((TotalSpaceMB / (SELECT SUM(TotalSpaceMB) FROM TableSizes) * 100), 2) AS NUMERIC(5, 2)) AS PercentOfTotal
FROM
    TableSizes
ORDER BY
    TotalSpaceMB DESC;
