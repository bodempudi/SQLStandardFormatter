SELECT
	CASE
		WHEN Status = 'A'
			THEN 'ACTIVE'
		WHEN Status = 'P'
			THEN 'PENDING'
		ELSE
			'OTHER'
	END AS StatusName
FROM dbo.Customer
