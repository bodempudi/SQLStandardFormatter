SELECT
	CASE
		WHEN Status = 'A'
			THEN
				CASE
					WHEN IsVip = 1
						THEN 'VIP ACTIVE'
					ELSE
						'ACTIVE'
				END
		ELSE
			'OTHER'
	END AS StatusName
FROM dbo.Customer
