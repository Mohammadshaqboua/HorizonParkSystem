/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Employee
{
    public string     EmployeeId        { get; set; }
    public string     Name              { get; set; }
    public Role       Role              { get; set; }
    public Assignment CurrentAssignment { get; set; }

    public override string ToString()
    {
        string assignmentInfo = CurrentAssignment != null
            ? $"{CurrentAssignment.RideOrFacilityId,-20}" +
              $"{CurrentAssignment.Shift,-12}" +
              $"{CurrentAssignment.AssignedAt,-20:yyyy-MM-dd HH:mm}"
            : $"{ "Not Assigned",-52}";

        return $"{EmployeeId,-10}" +
               $"{Name,-20}" +
               $"{Role,-22}" +
               $"{assignmentInfo}";
    }
}