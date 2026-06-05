if @Status='A'
begin
select 'ACTIVE'
end
else if @Status='P'
begin
select 'PENDING'
end
else
begin
select 'OTHER'
end
