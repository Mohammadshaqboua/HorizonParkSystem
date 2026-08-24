/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Assignment
{
    public string   RideOrFacilityId  { get; set; }
    public Shift    Shift             { get; set; } 
    public DateTime AssignedAt        { get; set; }

    public override string ToString()
    {
        return $"{RideOrFacilityId,-20}" +
               $"{Shift,-12}" +
               $"{AssignedAt,-20:yyyy-MM-dd HH:mm}";
    }
}