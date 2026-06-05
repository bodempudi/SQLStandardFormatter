begin transaction
update dbo.Employee set Salary=Salary+1000 where EmployeeId=1
commit transaction
