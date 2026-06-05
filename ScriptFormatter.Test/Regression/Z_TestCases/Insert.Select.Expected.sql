INSERT INTO dbo.CustomerArchive
(
	CustomerId
	,CustomerName
)
SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer
WHERE
	Status = 'I'
