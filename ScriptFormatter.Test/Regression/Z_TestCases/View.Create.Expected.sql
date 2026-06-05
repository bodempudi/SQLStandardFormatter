CREATE VIEW dbo.vw_ActiveCustomer
AS
SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer
WHERE
	Status = 'A'
