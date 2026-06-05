update dbo.Customer set CustomerName=@CustomerName,ModifiedDate=getdate() where CustomerId=@CustomerId
