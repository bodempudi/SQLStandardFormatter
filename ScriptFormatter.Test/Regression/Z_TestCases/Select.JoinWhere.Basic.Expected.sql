SELECT
	a.CustomerId
	,a.CustomerName
	,b.AccountNo
FROM dbo.Customer AS a
	INNER JOIN dbo.Account AS b
		ON a.CustomerId = b.CustomerId
		AND b.IsActive = 1
WHERE
	a.Status = 'ACTIVE'
	AND a.CreatedDate >= '2026-01-01'
