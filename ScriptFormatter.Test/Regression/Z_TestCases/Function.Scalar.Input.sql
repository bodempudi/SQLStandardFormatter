create function dbo.ufn_IsActive(@Status varchar(10)) returns bit as begin return case when @Status='A' then 1 else 0 end end
