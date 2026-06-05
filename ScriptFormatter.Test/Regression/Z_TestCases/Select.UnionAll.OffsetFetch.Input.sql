select CustomerId,CustomerName from dbo.Customer
union all
select CustomerId,CustomerName from dbo.CustomerArchive
order by CustomerName
offset 50 rows fetch next 25 rows only
