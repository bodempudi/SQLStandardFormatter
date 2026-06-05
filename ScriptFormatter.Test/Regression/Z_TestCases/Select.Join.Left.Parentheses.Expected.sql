SELECT
	c.CustomerId
	,c.CustomerName
	,a.AccountNo
FROM dbo.Customer AS c
	LEFT JOIN dbo.Account AS a
		ON c.CustomerId = a.CustomerId
		AND a.Status = 'ACTIVE'
WHERE
	(
		c.Status = 'ACTIVE'
		AND c.IsDeleted = 0
	)
	OR c.IsVip = 1
