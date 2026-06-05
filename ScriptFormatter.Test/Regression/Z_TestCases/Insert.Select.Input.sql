insert into dbo.CustomerArchive(CustomerId,CustomerName) select CustomerId,CustomerName from dbo.Customer where Status='I'
