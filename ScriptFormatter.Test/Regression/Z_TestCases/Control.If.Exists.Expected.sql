IF EXISTS
(
	SELECT
		1
	FROM dbo.Customer
	WHERE
		CustomerId = @CustomerId
)
BEGIN
	SELECT
		CustomerName
	FROM dbo.Customer
	WHERE
		CustomerId = @CustomerId
END
ELSE
BEGIN
	SELECT
		'NOT FOUND'
END
