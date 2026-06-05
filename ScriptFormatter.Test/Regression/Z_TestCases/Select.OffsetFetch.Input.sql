select CustomerId,CustomerName from dbo.Customer order by CustomerName offset 10 rows fetch next 20 rows only
