UPDATE dbo.Customer
SET CustomerName = @Name -- new value
WHERE CustomerId = @Id
