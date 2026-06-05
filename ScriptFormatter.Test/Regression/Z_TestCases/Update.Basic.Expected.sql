UPDATE dbo.Customer
SET
	CustomerName = @CustomerName
	,ModifiedDate = getdate()
WHERE
	CustomerId = @CustomerId
