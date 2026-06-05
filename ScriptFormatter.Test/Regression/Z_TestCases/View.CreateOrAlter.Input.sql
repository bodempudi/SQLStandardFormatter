create or alter view dbo.vw_ActiveCustomer as select CustomerId,CustomerName from dbo.Customer where Status='A'
