select case when Status='A' then case when IsVip=1 then 'VIP ACTIVE' else 'ACTIVE' end else 'OTHER' end StatusName from dbo.Customer
