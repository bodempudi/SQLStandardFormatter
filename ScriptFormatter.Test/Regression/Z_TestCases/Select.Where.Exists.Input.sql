select CustomerId from dbo.Customer c where exists(select 1 from dbo.Account a where a.CustomerId=c.CustomerId and a.IsActive=1)
