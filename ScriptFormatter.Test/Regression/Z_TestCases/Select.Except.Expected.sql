SELECT
	CustomerId
FROM dbo.Customer

EXCEPT

SELECT
	CustomerId
FROM dbo.CustomerArchive
