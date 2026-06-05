SELECT
	CustomerId
FROM dbo.Customer
WHERE
	CustomerId IN
	(
		SELECT
			CustomerId
		FROM dbo.Account
		WHERE
			IsActive = 1
	)
