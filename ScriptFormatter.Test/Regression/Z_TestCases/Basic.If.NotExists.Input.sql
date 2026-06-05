if not exists(select 1 from dbo.Customer where CustomerId=@CustomerId)
begin
insert into dbo.Customer(CustomerId,CustomerName)
values(@CustomerId,@CustomerName)
end