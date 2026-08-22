using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Assignment
{
    public string   RideOrFacilityId  { get; set; }
    public Shift    Shift { get; set; } 

    public DateTime AssignedAt        { get; set; }
}