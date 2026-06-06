;WITH CustomerCTE AS
(
	SELECT
		CustomerId
		,CustomerName
	FROM dbo.Customer
	WHERE
		Status = 'ACTIVE'
)
,AccountCTE AS
(
	SELECT
		CustomerId
		,AccountNo
	FROM dbo.Account
	WHERE
		IsActive = 1
)
SELECT
	c.CustomerName
	,a.AccountNo
FROM CustomerCTE AS c
	INNER JOIN AccountCTE AS a
		ON c.CustomerId = a.CustomerId
