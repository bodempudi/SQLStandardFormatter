CREATE FUNCTION dbo.ufn_ActiveCustomers()
RETURNS TABLE
AS
RETURN
(
	SELECT
		CustomerId
		,CustomerName
	FROM dbo.Customer
	WHERE
		Status = 'A'
)
