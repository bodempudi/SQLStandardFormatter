CREATE FUNCTION dbo.ufn_IsActive
(
	@Status VARCHAR(10)
)
RETURNS BIT
AS
BEGIN
	RETURN
		CASE
			WHEN @Status = 'A'
				THEN 1
			ELSE
				0
		END
END
