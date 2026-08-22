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
}