create or alter procedure dbo.usp_SaveCustomer
@CustomerId int,
@StatusCode varchar(30) output
as
set @StatusCode='0'
return
