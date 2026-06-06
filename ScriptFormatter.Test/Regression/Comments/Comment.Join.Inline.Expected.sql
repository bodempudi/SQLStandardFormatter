SELECT
	*
FROM Customer AS c
	INNER JOIN Account AS a -- account join
		ON c.CustomerId = a.CustomerId
