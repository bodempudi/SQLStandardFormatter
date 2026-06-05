WHILE @Counter < 10
BEGIN
	SET @Counter = @Counter + 1

	SELECT
		@Counter
END
