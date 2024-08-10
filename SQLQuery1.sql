﻿Create type ParamExpType as Table
(
	Title NVARCHAR(MAX),
	Duration int
)
GO 

Create Or Alter Procedure InsertEmployeeSP
	
	@Name NVARCHAR(MAX),
	@IsActive bit,
	@JoinDate Date,
	@ImageName NVARCHAR(MAX),
	@ImageUrl NVARCHAR(MAX),
	@Salary int,
	@Exp ParamExpType Readonly
as
Begin 
	Set Nocount on;

	Declare @LocalExp Table 
	(
		EmployeeId int,
		Title NVARCHAR(MAX),
		Duration int
	)
	Declare @EmployeeId int

	Insert Into Employees (Name ,IsActive ,JoinDate ,ImageName ,ImageUrl,Salary)
	Values (@Name ,@IsActive ,@JoinDate ,@ImageName ,@ImageUrl,@Salary);

	Set @EmployeeId = SCOPE_IDENTITY();

	Insert into @LocalExp (EmployeeId ,	Title ,	Duration )
	Select @EmployeeId , Title,	Duration From @Exp;

	Insert into Experiences (EmployeeId ,	Title ,	Duration)
	Select @EmployeeId , Title,	Duration From @LocalExp

End