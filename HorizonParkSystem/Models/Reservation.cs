/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Reservation
{
    public string            ReservationId { get; set; }
    public string            VisitorId     { get; set; }
    public string            RideId        { get; set; }
    public string            TimeSlot      { get; set; }
    public ReservationStatus Status        { get; set; }
    public DateTime          CreatedAt     { get; set; }

    public override string ToString()
    {
        return $"{ReservationId,-14}" +
               $"{VisitorId,-14}" +
               $"{RideId,-12}" +
               $"{TimeSlot,-12}" +
               $"{Status,-14}" +
               $"{CreatedAt,-20:yyyy-MM-dd HH:mm}";
    }
}