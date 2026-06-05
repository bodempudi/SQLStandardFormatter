CREATE PROCEDURE dbo.usp_GetCustomer
	@CustomerId INT
AS
SELECT
	CustomerId
	,CustomerName
FROM dbo.Customer
WHERE
	CustomerId = @CustomerId
