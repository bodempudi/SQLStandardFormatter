SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer
ORDER BY
	CustomerName
OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY
