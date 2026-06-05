select case Status when 'A' then 'ACTIVE' when 'P' then 'PENDING' else 'OTHER' end StatusName from dbo.Customer
