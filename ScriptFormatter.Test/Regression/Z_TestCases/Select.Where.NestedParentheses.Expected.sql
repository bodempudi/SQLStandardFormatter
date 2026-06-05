SELECT
	*
FROM dbo.Customer
WHERE
	(
		(
			Status = 'ACTIVE'
			AND IsDeleted = 0
		)
		OR
		(
			Status = 'PENDING'
			AND IsVip = 1
		)
	)
	AND CustomerType = 'RETAIL'
