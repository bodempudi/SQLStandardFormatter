select case when Status='A' then 'ACTIVE' when Status='P' then 'PENDING' else 'OTHER' end StatusName from dbo.Customer
