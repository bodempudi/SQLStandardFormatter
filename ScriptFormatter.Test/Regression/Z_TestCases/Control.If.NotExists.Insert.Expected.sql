IF NOT EXISTS
(
	SELECT
		1
	FROM dbo.Customer
	WHERE
		CustomerId = @CustomerId
)
BEGIN
	INSERT INTO dbo.Customer
	(
		CustomerId
		,CustomerName
	)
	VALUES
	(
		@CustomerId
		,@CustomerName
	)
END
