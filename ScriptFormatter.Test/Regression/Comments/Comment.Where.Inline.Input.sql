SELECT *
FROM dbo.Customer
WHERE CustomerId = @CustomerId -- lookup customer
  AND Status = 'A' -- active only
