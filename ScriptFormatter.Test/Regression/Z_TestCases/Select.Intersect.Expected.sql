SELECT
	CustomerId
FROM dbo.Customer

INTERSECT

SELECT
	CustomerId
FROM dbo.Account
