UPDATE c
SET
	c.Status = 'I'
FROM dbo.Customer AS c
	INNER JOIN dbo.Account AS a
		ON c.CustomerId = a.CustomerId
WHERE
	a.IsClosed = 1
