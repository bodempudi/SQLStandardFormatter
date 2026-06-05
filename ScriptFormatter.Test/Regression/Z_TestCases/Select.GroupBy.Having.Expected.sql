SELECT
	CustomerType
	,count(*) AS TotalCustomers
FROM dbo.Customer
GROUP BY
	CustomerType
HAVING
	count(*) > 10
