select * from dbo.Customer
where
(
    (Status='ACTIVE' and IsDeleted=0)
    or
    (Status='PENDING' and IsVip=1)
)
and CustomerType='RETAIL'
