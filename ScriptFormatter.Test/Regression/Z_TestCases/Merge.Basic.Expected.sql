MERGE dbo.CustomerTarget
USING dbo.CustomerSource AS s
ON t.CustomerId = s.CustomerId
WHEN MATCHED THEN
	UPDATE
	SET
		t.CustomerName = s.CustomerName
WHEN NOT MATCHED THEN
	INSERT
	(
		CustomerId
		,CustomerName
	)
	VALUES
	(
		s.CustomerId
		,s.CustomerName
	);
