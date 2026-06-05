select a.CustomerId,a.CustomerName,b.AccountNo from dbo.Customer a inner join dbo.Account b on a.CustomerId=b.CustomerId and b.IsActive=1 where a.Status='ACTIVE' and a.CreatedDate>='2026-01-01'
