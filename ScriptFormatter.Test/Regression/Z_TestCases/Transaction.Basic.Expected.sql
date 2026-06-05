BEGIN TRANSACTION

	UPDATE dbo.Employee
	SET
		Salary = Salary + 1000
	WHERE
		EmployeeId = 1

COMMIT TRANSACTION
