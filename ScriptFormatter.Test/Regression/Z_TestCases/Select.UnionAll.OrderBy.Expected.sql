SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer
WHERE
	Status = 'ACTIVE'

UNION ALL

SELECT
	CustomerId
	,CustomerName
FROM dbo.CustomerArchive
WHERE
	Status = 'ACTIVE'
ORDER BY
	CustomerName DESC
