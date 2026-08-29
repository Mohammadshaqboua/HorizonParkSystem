/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Models;
using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Services;

public class ParkSystemService
{
    private Visitor[] _visitors = new Visitor[0];
    private Ride[] _rides = new Ride[0];
    private Employee[] _employees = new Employee[0];
    private Reservation[] _reservations = new Reservation[0];
    private Ticket[] _tickets = new Ticket[0];
    
    private readonly string[] _knownFacilities = new string[]
    {
        "Main Gate", "Ticket Booth A", "Ticket Booth B", "First Aid", "Food Court"
    };

    private int _ticketCounter = 1;
    private int _reservationCounter = 1;
    private int _visitorCounter = 1;
    private int _rideCounter = 1;
    private int _employeeCounter = 1;
    private static void AddToArray<T>(ref T[] array, T item)
    {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = item;
    }

    public (bool Success, string Message) RegisterVisitor(
        string name,
        int age,
        int heightCm,
        VisitorCategory category,
        bool hasAccompanyingAdult)
    {
        if (age < 0 || age > 120)
        {
            return (false, "Registration failed: Age must be between 0 and 120.");
        }

        if (heightCm < 50 || heightCm > 200)
        {
            return (false, "Registration failed: height must be between 50 and 200.");
        }

        string visitorId = $"V-{_visitorCounter++}";

        Visitor visitor = new Visitor(visitorId, name, age, heightCm, category,hasAccompanyingAdult);

        AddToArray(ref _visitors, visitor);
        return (true, $"Visitor '{name}' registered successfully with ID {visitorId}.");
    }

    public (bool Success, string Message) IssueTicket(string visitorId, TicketType type, string[] allowedRideIds)
    {
        Visitor visitor = null;
        foreach (var v in _visitors)
        {
            if (v.VisitorId == visitorId)
            {
                visitor = v;
                break;
            }
        }

        if (visitor == null)
        {
            return (false, $"Issue ticket failed: Visitor '{visitorId}' not found.");
        }

        allowedRideIds = allowedRideIds ?? new string[0];

        string[] invalidRideIds = new string[0];
        foreach (var id in allowedRideIds)
        {
            bool rideExists = false;
            foreach (var r in _rides)
            {
                if (r.RideId == id)
                {
                    rideExists = true;
                    break;
                }
            }

            if (!rideExists)
            {
                AddToArray(ref invalidRideIds, id);
            }
        }

        if (invalidRideIds.Length > 0)
        {
            return (false, $"Issue ticket failed: The following Ride IDs do not exist: {string.Join(", ", invalidRideIds)}");
        }

        if (!IsTicketTypeAllowedForCategory(visitor.Category,type))
        {
            return (false, "Issue ticket failed: Ticket type does not match visitor category.");
        }

        if (visitor.ActiveTicket != null && visitor.ActiveTicket.Status == TicketStatus.Active)
        {
            return (false, $"Issue ticket failed: Visitor already has an active ticket ({visitor.ActiveTicket.TicketId}).");
        }

        decimal price = GetPriceForTicketType(type);

        var ticket = new Ticket
        {
            TicketId = $"T-{_ticketCounter++}",
            Type = type,
            Price = price,
            IssueDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddDays(1),
            Status = TicketStatus.Active,
            AllowedRideIds = allowedRideIds
        };

        AddToArray(ref _tickets, ticket);
        visitor.ActiveTicket = ticket;

        return (true, $"Ticket {ticket.TicketId} issued to {visitor.Name}. Price: {price:C}");
    }

    public (bool Success, string Message) DeactivateTicket(string visitorId)
    {
        Visitor visitor = null;
        foreach (var v in _visitors)
        {
            if (v.VisitorId == visitorId)
            {
                visitor = v;
                break;
            }
        }

        if (visitor == null)
        {
            return (false, $"Deactivate ticket failed: Visitor '{visitorId}' not found.");
        }

        if (visitor.ActiveTicket == null)
        {
            return (false, $"Deactivate ticket failed: Visitor '{visitorId}' has no ticket.");
        }

        if (visitor.ActiveTicket.Status == TicketStatus.Cancelled)
        {
            return (false, $"Deactivate ticket failed: Ticket {visitor.ActiveTicket.TicketId} is already cancelled.");
        }

        visitor.ActiveTicket.Status = TicketStatus.Cancelled;

        return (true, $"Ticket {visitor.ActiveTicket.TicketId} deactivated for visitor '{visitor.Name}'.");
    }

    public (bool Success, string Message) ValidateTicket(string visitorId)
    {
        Visitor visitor = null;
        foreach (var v in _visitors)
        {
            if (v.VisitorId == visitorId)
            {
                visitor = v;
                break;
            }
        }

        if (visitor == null)
        {
            return (false, $"Validation failed: Visitor '{visitorId}' not found.");
        }

        if (visitor.ActiveTicket == null)
        {
            return (false, "Access denied: Visitor has no ticket.");
        }

        if (visitor.ActiveTicket.Status == TicketStatus.Cancelled)
        {
            return (false, "Access denied: Ticket has been cancelled.");
        }

        if (!visitor.ActiveTicket.IsValid())
        {
            return (false, "Access denied: Ticket is expired.");
        }

        return (true, "Ticket is valid.");
    }

    public (bool Success, string Message) CheckRideAccess(string visitorId, string rideId, bool hasAccompanyingAdult)
    {
        Visitor visitor = null;
        foreach (var v in _visitors)
        {
            if (v.VisitorId == visitorId)
            {
                visitor = v;
                break;
            }
        }
        if (visitor == null)
        {
            return (false, $"Access check failed: Visitor '{visitorId}' not found.");
        }
        Ride ride = null;
        foreach (var r in _rides)
        {
            if (r.RideId == rideId)
            {
                ride = r;
                break;
            }
        }
        if (ride == null)
        {
            return (false, $"Access check failed: Ride '{rideId}' not found.");
        }
        var ticketCheck = ValidateTicket(visitorId);
        if (!ticketCheck.Success)
        {
            return ticketCheck;
        }
        if (!ride.IsOpen())
        {
            return (false, $"Access denied: Ride '{ride.Name}' is currently {ride.Status}.");
        }
        if (!visitor.ActiveTicket.GrantsAccessToAllRides())
        {
            bool allowed = false;
            foreach (var id in visitor.ActiveTicket.AllowedRideIds)
            {
                if (id == rideId)
                {
                    allowed = true;
                    break;
                }
            }
            if (!allowed)
            {
                return (false, $"Access denied: Ticket does not include access to '{ride.Name}'.");
            }
        }
        var eligibility = ride.CheckEligibility(visitor, hasAccompanyingAdult);
        if (!eligibility.IsEligible)
        {
            return (false, $"Access denied: {eligibility.Reason}");
        }
        if (!ride.HasAvailableCapacity())
        {
            return (false, $"Access denied: Ride '{ride.Name}' is at full capacity.");
        }
        ride.CurrentOccupancy++;
        return (true, $"Access granted to '{ride.Name}'.");
    }

    public (bool Success, string Message) CreateReservation(string visitorId, string rideId, string timeSlot)
    {
        Visitor visitor = null;
        foreach (var v in _visitors)
        {
            if (v.VisitorId == visitorId)
            {
                visitor = v;
                break;
            }
        }

        if (visitor == null)
        {
            return (false, $"Reservation failed: Visitor '{visitorId}' not found.");
        }

        Ride ride = null;
        foreach (var r in _rides)
        {
            if (r.RideId == rideId)
            {
                ride = r;
                break;
            }
        }

        if (ride == null)
        {
            return (false, $"Reservation failed: Ride '{rideId}' not found.");
        }

        if (!ride.IsOpen())
        {
            return (false, $"Reservation failed: Ride '{ride.Name}' is currently {ride.Status}.");
        }
        
        if (!TimeSpan.TryParse(timeSlot, out _))
        {
            return (false, $"Reservation failed: '{timeSlot}' is not a valid time format (expected HH:mm).");
        }
        
        var ticketCheck = ValidateTicket(visitorId);
        if (!ticketCheck.Success)
        {
            return ticketCheck;
        }
        
        var eligibility = ride.CheckEligibility(visitor, visitor.HasAccompanyingAdult);
        if (!eligibility.IsEligible)
        {
            return (false, $"Reservation failed: {eligibility.Reason}");
        }

        bool alreadyReserved = false;
        foreach (var r in _reservations)
        {
            if (r.VisitorId == visitorId &&
                r.RideId == rideId &&
                r.TimeSlot == timeSlot &&
                r.Status == ReservationStatus.Active)
            {
                alreadyReserved = true;
                break;
            }
        }

        if (alreadyReserved)
        {
            return (false, "Reservation failed: Visitor already has a reservation for this ride and time slot.");
        }

        int reservedCount = 0;
        foreach (var r in _reservations)
        {
            if (r.RideId == rideId &&
                r.TimeSlot == timeSlot &&
                r.Status == ReservationStatus.Active)
            {
                reservedCount++;
            }
        }

        if (reservedCount >= ride.MaxCapacity)
        {
            return (false, "Reservation failed: Ride has reached maximum capacity for the selected time slot.");
        }

        var reservation = new Reservation
        {
            ReservationId = $"R-{_reservationCounter++}",
            VisitorId = visitorId,
            RideId = rideId,
            TimeSlot = timeSlot,
            Status = ReservationStatus.Active,
            CreatedAt = DateTime.Now
        };

        AddToArray(ref _reservations, reservation);

        return (true, $"Reservation {reservation.ReservationId} created for '{ride.Name}' at {timeSlot}.");
    }

    public (bool Success, string Message) CancelReservation(string reservationId)
    {
        Reservation reservation = null;
        foreach (var r in _reservations)
        {
            if (r.ReservationId == reservationId)
            {
                reservation = r;
                break;
            }
        }

        if (reservation == null)
        {
            return (false, $"Cancel failed: Reservation '{reservationId}' not found.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return (false, "Cancel failed: Reservation is already cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        return (true, $"Reservation {reservationId} cancelled successfully.");
    }

    public (bool Success, string Message) AddRide(Ride ride)
    {
        if (ride.MinAge < 0)
        {
            return (false, "Add ride failed: Minimum age cannot be negative.");
        }

        if (ride.MinHeightCm < 0)
        {
            return (false, "Add ride failed: Minimum height cannot be negative.");
        }

        if (ride.MaxCapacity <= 0)
        {
            return (false, "Add ride failed: Max capacity must be greater than zero.");
        }

        ride.RideId = $"RIDE-{_rideCounter++}";

        AddToArray(ref _rides, ride);

        return (true, $"Ride '{ride.Name}' added successfully with ID {ride.RideId}.");
    }

    public (bool Success, string Message) UpdateRideStatus(string rideId, RideStatus newStatus)
    {
        Ride ride = null;
        foreach (var r in _rides)
        {
            if (r.RideId == rideId)
            {
                ride = r;
                break;
            }
        }

        if (ride == null)
        {
            return (false, $"Update ride status failed: Ride '{rideId}' not found.");
        }

        ride.Status = newStatus;

        return (true, $"Ride '{ride.Name}' status updated to {newStatus}.");
    }

    public (bool Success, string Message) AssignEmployee(string employeeId, string rideOrFacilityId, Shift shift)
    {
        Employee employee = null;
        foreach (var e in _employees)
        {
            if (e.EmployeeId == employeeId)
            {
                employee = e;
                break;
            }
        }

        if (employee == null)
        {
            return (false, $"Assignment failed: Employee '{employeeId}' not found.");
        }

        bool isValidRide = false;
        foreach (var r in _rides)
        {
            if (r.RideId == rideOrFacilityId)
            {
                isValidRide = true;
                break;
            }
        }

        bool isValidFacility = false;
        foreach (var facility in _knownFacilities)
        {
            if (facility == rideOrFacilityId)
            {
                isValidFacility = true;
                break;
            }
        }

        if (!isValidRide && !isValidFacility)
        {
            return (false, $"Assignment failed: '{rideOrFacilityId}' is not a recognized ride or facility.");
        }

        if (employee.CurrentAssignment != null && employee.CurrentAssignment.Shift == shift)
        {
            return (false, $"Assignment failed: Employee is already assigned to '{employee.CurrentAssignment.RideOrFacilityId}' during shift '{shift}'.");
        }

        employee.CurrentAssignment = new Assignment
        {
            RideOrFacilityId = rideOrFacilityId,
            Shift = shift,
            AssignedAt = DateTime.Now
        };

        return (true, $"Employee '{employee.Name}' assigned to '{rideOrFacilityId}' for shift '{shift}'.");
    }

    public string GetRideOccupancyStatus(string rideId)
    {
        Ride ride = null;
        foreach (var r in _rides)
        {
            if (r.RideId == rideId)
            {
                ride = r;
                break;
            }
        }

        if (ride == null)
        {
            return $"Ride '{rideId}' not found.";
        }

        return $"{ride.Name}: {ride.CurrentOccupancy}/{ride.MaxCapacity} | Status: {ride.Status}";
    }

    public (bool Success, string Message) RegisterEmployee(string name, Role role)
    {
        string employeeId = $"E-{_employeeCounter++}";

        var employee = new Employee
        {
            EmployeeId = employeeId,
            Name = name,
            Role = role
        };

        AddToArray(ref _employees, employee);

        return (true, $"Employee '{name}' registered successfully with ID {employeeId}.");
    }

    public string GetInfo(EntitySector entitySector, string id)
    {
        if (entitySector == EntitySector.Visitor)
        {
            Visitor visitor = null;
            foreach (var v in _visitors)
            {
                if (v.VisitorId == id)
                {
                    visitor = v;
                    break;
                }
            }

            if (visitor == null)
                return $"Visitor '{id}' not found.";

            string ticketInfo = visitor.ActiveTicket == null
                ? "No active ticket"
                : $"{visitor.ActiveTicket.TicketId} ({visitor.ActiveTicket.Status})";

            return $"[VISITOR INFO]\n" +
                   $"  ID:       {visitor.VisitorId}\n" +
                   $"  Name:     {visitor.Name}\n" +
                   $"  Age:      {visitor.Age}\n" +
                   $"  Height:   {visitor.HeightCm} cm\n" +
                   $"  Category: {visitor.Category}\n" +
                   $"  Accompanying Adult: {visitor.HasAccompanyingAdult}\n"+
                   $"  Ticket:   {ticketInfo}";
        }

        if (entitySector == EntitySector.Ride)
        {
            Ride ride = null;
            foreach (var r in _rides)
            {
                if (r.RideId == id)
                {
                    ride = r;
                    break;
                }
            }

            if (ride == null)
                return $"Ride '{id}' not found.";

            return $"[RIDE INFO]\n" +
                   $"  ID:         {ride.RideId}\n" +
                   $"  Name:       {ride.Name}\n" +
                   $"  Type:       {ride.Type}\n" +
                   $"  Min Age:    {ride.MinAge}\n" +
                   $"  Min Height: {ride.MinHeightCm} cm\n" +
                   $"  Occupancy:  {ride.CurrentOccupancy}/{ride.MaxCapacity}\n" +
                   $"  Status:     {ride.Status}";
        }

        if (entitySector == EntitySector.Employee)
        {
            Employee employee = null;
            foreach (var e in _employees)
            {
                if (e.EmployeeId == id)
                {
                    employee = e;
                    break;
                }
            }

            if (employee == null)
                return $"Employee '{id}' not found.";

            string assignmentInfo = employee.CurrentAssignment == null
                ? "Not assigned"
                : $"{employee.CurrentAssignment.RideOrFacilityId} ({employee.CurrentAssignment.Shift})";

            return $"[EMPLOYEE INFO]\n" +
                   $"  ID:         {employee.EmployeeId}\n" +
                   $"  Name:       {employee.Name}\n" +
                   $"  Role:       {employee.Role}\n" +
                   $"  Assignment: {assignmentInfo}";
        }

        if (entitySector == EntitySector.Ticket)
        {
            Ticket ticket = null;
            foreach (var t in _tickets)
            {
                if (t.TicketId == id)
                {
                    ticket = t;
                    break;
                }
            }

            if (ticket == null)
                return $"Ticket '{id}' not found.";

            return $"[TICKET INFO]\n" +
                   $"  ID:     {ticket.TicketId}\n" +
                   $"  Type:   {ticket.Type}\n" +
                   $"  Price:  {ticket.Price:C}\n" +
                   $"  Status: {ticket.Status}\n" +
                   $"  Expiry: {ticket.ExpiryDate:g}";
        }

        if (entitySector == EntitySector.Reservation)
        {
            Reservation reservation = null;
            foreach (var r in _reservations)
            {
                if (r.ReservationId == id)
                {
                    reservation = r;
                    break;
                }
            }

            if (reservation == null)
                return $"Reservation '{id}' not found.";

            return $"[RESERVATION INFO]\n" +
                   $"  ID:        {reservation.ReservationId}\n" +
                   $"  Visitor:   {reservation.VisitorId}\n" +
                   $"  Ride:      {reservation.RideId}\n" +
                   $"  Time Slot: {reservation.TimeSlot}\n" +
                   $"  Status:    {reservation.Status}";
        }

        return $"No record found for sector '{entitySector}' with ID '{id}'.";
    }

    public void DisplayAllData (EntitySector sector)
    {
        switch (sector)
        {
            case EntitySector.Visitor:
            {
                Console.WriteLine(
                    $"{"ID",-10}" +
                    $"{"Name",-20}" +
                    $"{"Age",-6}" +
                    $"{"Height",-10}" +
                    $"{"Category",-15}" +
                    $"{"Accompanying Adult",-22}" +
                    $"{"Active Ticket",-15}"
                );

                Console.WriteLine(new string('-', 91));

                foreach (var visitor in _visitors)
                {
                    Console.WriteLine(visitor.ToString());
                }
                break;
            }
            case EntitySector.Ride:
            {
                Console.WriteLine(
                    $"{"ID",-8}" +
                    $"{"Name",-20}" +
                    $"{"Type",-12}" +
                    $"{"Status",-10}" +
                    $"{"Age",-4}" +
                    $"{"Height",-8}" +
                    $"{"Occupancy",-16}" +
                    $"{"Requirement",-18}"
                );

                Console.WriteLine(new string('-', 96));

                foreach (var ride in _rides)
                {
                    Console.WriteLine(ride.ToString());
                }
                
                break;
            }
            case EntitySector.Employee:
            {
                Console.WriteLine(
                    $"{"Employee ID",-10}" +
                    $"{"Name",-20}" +
                    $"{"Role",-22}" +
                    $"{"Ride / Facility",-20}" +
                    $"{"Shift",-12}" +
                    $"{"Assigned At",-20}");

                Console.WriteLine(new string('-', 104));
                
                foreach (var employee in _employees)
                {
                    Console.WriteLine(employee.ToString());
                }
                
                break;
            }
            case EntitySector.Ticket:
            {
                Console.WriteLine(
                    $"{"Ticket ID",-10}" +
                    $"{"Type",-12}" +
                    $"{"Price",-10}" +
                    $"{"Issue Date",-14}" +
                    $"{"Expiry Date",-14}" +
                    $"{"Status",-12}" +
                    $"{"Allowed Rides",-25}");

                Console.WriteLine(new string('-', 97));
                
                foreach (var ticket in _tickets)
                {
                    Console.WriteLine(ticket.ToString());
                }
                
                break;
            }
            case EntitySector.Reservation:
            {
                Console.WriteLine(
                    $"{"ReservationID",-14}" +
                    $"{"VisitorID",-14}" +
                    $"{"RideID",-12}" +
                    $"{"Time Slot",-12}" +
                    $"{"Status",-14}" +
                    $"{"Created At",-20}");

                Console.WriteLine(new string('-', 96));
                
                foreach (var reservation in _reservations)
                {
                    Console.WriteLine(reservation.ToString());
                }
                
                break;
            }
        }
    }

    public Visitor GetVisitor(string id)
    {
        foreach (var visitor in _visitors)
        {
            if (visitor.VisitorId == id)
            {
                return visitor;
            }
        }
        return null;
    }
    
    private bool IsTicketTypeAllowedForCategory(VisitorCategory category, TicketType type)
    {
        switch (category)
        {
            case VisitorCategory.Child:
                return type == TicketType.Child;
            case VisitorCategory.Senior:
                return type == TicketType.Senior;
            case VisitorCategory.VIP:
                return type == TicketType.VIP;
            case VisitorCategory.General:
                return type == TicketType.Regular || type == TicketType.VIP;
            case VisitorCategory.StaffAccompaniedMinor:
                return type == TicketType.Child;
            default:
                return false;
        }
    }
    
    private decimal GetPriceForTicketType(TicketType type)
    {
        return type switch
        {
            TicketType.VIP => 100m,
            TicketType.Regular => 50m,
            TicketType.Child => 30m,
            TicketType.Senior => 35m,
            _ => 50m
        };
    }
}