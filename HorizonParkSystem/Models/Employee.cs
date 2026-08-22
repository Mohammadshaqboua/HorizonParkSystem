using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Employee
{
    public string     EmployeeId        { get; set; }
    public string     Name              { get; set; }
    public Role       Role              { get; set; }

    public Assignment CurrentAssignment { get; set; }
}