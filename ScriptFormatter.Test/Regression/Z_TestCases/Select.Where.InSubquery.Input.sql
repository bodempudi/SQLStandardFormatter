select CustomerId from dbo.Customer where CustomerId in (select CustomerId from dbo.Account where IsActive=1)
