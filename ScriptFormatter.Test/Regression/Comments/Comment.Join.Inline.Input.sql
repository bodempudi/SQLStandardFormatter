SELECT *
FROM Customer c
INNER JOIN Account a -- account join
    ON c.CustomerId = a.CustomerId
