create function dbo.ufn_ActiveCustomers() returns table as return (select CustomerId,CustomerName from dbo.Customer where Status='A')
