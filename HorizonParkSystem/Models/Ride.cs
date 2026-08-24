/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Ride
{
    public string            RideId                    { get; set; }
    public string            Name                      { get; set; }
    public RideType          Type                      { get; set; }
    public int               MinAge                    { get; set; }
    public int               MinHeightCm               { get; set; }
    public bool              RequiresAccompanyingAdult { get; set; }
    public int               MaxCapacity               { get; set; }
    public int               CurrentOccupancy          { get; set; }
    public RideStatus        Status                    { get; set; }

    public bool IsOpen()
    {
        return Status == RideStatus.Open;
    }

    public bool HasAvailableCapacity()
    {
        return CurrentOccupancy < MaxCapacity;
    }

    public EligibilityResult CheckEligibility(Visitor visitor)
    {
        if (visitor.Age < MinAge)
            return new EligibilityResult
            {
                IsEligible = false,
                Reason = $"Visitor does not meet the minimum age requirement ({MinAge})."
            };

        if (visitor.HeightCm < MinHeightCm)
            return new EligibilityResult
            {
                IsEligible = false,
                Reason = $"Visitor does not meet the minimum height requirement ({MinHeightCm}cm)."
            };
        
        return new EligibilityResult { IsEligible = true, Reason = "Eligible" };
    }

    public override string ToString()
    {
        return $"{RideId,-8}" +
               $"{Name,-20}" +
               $"{Type,-12}" +
               $"{Status,-10}" +
               $"{MinAge,-4}" +
               $"{(MinHeightCm + " cm"),-8}" +
               $"{($"{CurrentOccupancy}/{MaxCapacity}"),-16}" +
               $"{(RequiresAccompanyingAdult ? "Adult Required" : "False"),-18}";
    }
}