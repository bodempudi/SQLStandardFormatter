SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer

UNION ALL

SELECT
	CustomerId
	,CustomerName
FROM dbo.CustomerArchive
ORDER BY
	CustomerName
OFFSET 50 ROWS FETCH NEXT 25 ROWS ONLY
