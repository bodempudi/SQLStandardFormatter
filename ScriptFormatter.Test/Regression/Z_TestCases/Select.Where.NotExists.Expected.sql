SELECT
	CustomerId
FROM dbo.Customer AS c
WHERE NOT EXISTS
(
	SELECT
		1
	FROM dbo.Account AS a
	WHERE
		a.CustomerId = c.CustomerId
)
