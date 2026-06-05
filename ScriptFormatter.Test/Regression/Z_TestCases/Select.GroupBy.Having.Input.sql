select CustomerType,count(*) TotalCustomers from dbo.Customer group by CustomerType having count(*)>10
