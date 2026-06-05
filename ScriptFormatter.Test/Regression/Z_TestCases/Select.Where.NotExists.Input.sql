select CustomerId from dbo.Customer c where not exists(select 1 from dbo.Account a where a.CustomerId=c.CustomerId)
