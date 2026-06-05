update c set c.Status='I' from dbo.Customer c inner join dbo.Account a on c.CustomerId=a.CustomerId where a.IsClosed=1
