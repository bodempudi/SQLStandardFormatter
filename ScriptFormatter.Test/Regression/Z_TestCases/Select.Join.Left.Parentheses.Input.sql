select c.CustomerId,c.CustomerName,a.AccountNo from dbo.Customer c left join dbo.Account a on c.CustomerId=a.CustomerId and a.Status='ACTIVE' where (c.Status='ACTIVE' and c.IsDeleted=0) or c.IsVip=1
