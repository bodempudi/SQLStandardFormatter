SELECT
	CASE Status
		WHEN 'A'
			THEN 'ACTIVE'
		WHEN 'P'
			THEN 'PENDING'
		ELSE
			'OTHER'
	END AS StatusName
FROM dbo.Customer
