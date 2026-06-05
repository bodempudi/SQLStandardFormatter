create procedure dbo.usp_GetCustomer
@CustomerId int
as
select CustomerId,CustomerName from dbo.Customer where CustomerId=@CustomerId
