begin try
begin transaction
update dbo.Employee set Salary=Salary+1000 where EmployeeId=1
commit transaction
end try
begin catch
rollback transaction
throw
end catch
