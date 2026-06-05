CREATE OR ALTER PROCEDURE dbo.usp_SaveCustomer
	@CustomerId INT
	,@StatusCode VARCHAR(30) OUTPUT
AS
SET @StatusCode = '0'

RETURN
