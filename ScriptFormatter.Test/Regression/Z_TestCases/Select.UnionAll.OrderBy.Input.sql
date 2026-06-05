select CustomerId,CustomerName from dbo.Customer where Status='ACTIVE'
union all
select CustomerId,CustomerName from dbo.CustomerArchive where Status='ACTIVE'
order by CustomerName desc
