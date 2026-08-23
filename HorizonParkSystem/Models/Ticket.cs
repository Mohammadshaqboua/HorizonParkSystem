/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Ticket
{
    public string       TicketId       { get; set; }
    public TicketType   Type           { get; set; }
    public decimal      Price          { get; set; }
    public DateTime     IssueDate      { get; set; }
    public DateTime     ExpiryDate     { get; set; }
    public TicketStatus Status         { get; set; }
    public string[]     AllowedRideIds { get; set; } = new string[0];
    public bool IsValid()
    {
        return Status == TicketStatus.Active && DateTime.Now <= ExpiryDate;
    }

    public bool GrantsAccessToAllRides()
    {
        return Type == TicketType.VIP;
    }
}