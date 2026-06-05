if exists(select 1 from dbo.Customer where CustomerId=@CustomerId)
begin
select CustomerName
from dbo.Customer
where CustomerId=@CustomerId
end
else
begin
select 'NOT FOUND'
end
