using HorizonParkSystem.Models;
using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Services;

public class ParkSystemService
{
    private List<Visitor> _visitors = new List<Visitor>();
    private List<Ride> _rides = new List<Ride>();
    private List<Employee> _employees = new List<Employee>();
    private List<Reservation> _reservations = new List<Reservation>();
    private List<Ticket> _tickets = new List<Ticket>();
    private readonly List<string> _knownFacilities = new List<string>
    {
        "Main Gate", "Ticket Booth A", "Ticket Booth B", "First Aid", "Food Court"
    };

    private int _ticketCounter = 1;
    private int _reservationCounter = 1;
    private int _visitorCounter = 1;    
    private int _rideCounter = 1;        
    private int _employeeCounter = 1;

    public (bool Success, string Message) RegisterVisitor(
        string name,
        int age,
        int heightCm,
        VisitorCategory category)
    {
        if (age < 0 || age > 120)
        {
            return (false, "Registration failed: Age must be between 0 and 120.");
        }

        if (heightCm  < 50 || heightCm > 200)
        {
            return (false, "Registration failed: height must be between 50 and 200.");
        }
        
        string visitorId = $"V-{_visitorCounter++}";
        
        Visitor visitor = new Visitor(visitorId, name, age, heightCm, category);

        _visitors.Add(visitor);
        return (true, $"Visitor '{name}' registered successfully with ID {visitorId}.");
    }

    public (bool Success, string Message) IssueTicket(string visitorId, TicketType type, List<string> allowedRideIds)
    {
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == visitorId);
        if (visitor == null)
        {
            return (false, $"Issue ticket failed: Visitor '{visitorId}' not found.");
        }

        allowedRideIds = allowedRideIds ?? new List<string>(); 

        var invalidRideIds = allowedRideIds
            .Where(id => !_rides.Any(r => r.RideId == id))
            .ToList();

        if (invalidRideIds.Any())
        {
            return (false, $"Issue ticket failed: The following Ride IDs do not exist: {string.Join(", ", invalidRideIds)}");
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

        _tickets.Add(ticket);
        visitor.ActiveTicket = ticket;

        return (true, $"Ticket {ticket.TicketId} issued to {visitor.Name}. Price: {price:C}");
    }

    public (bool Success, string Message) DeactivateTicket(string visitorId)
    {
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == visitorId);
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
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == visitorId);
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

    public (bool Success, string Message) CheckRideAccess(string visitorId, string rideId)
    {
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == visitorId);
        if (visitor == null)
        {
            return (false, $"Access check failed: Visitor '{visitorId}' not found.");
        }

        var ride = _rides.FirstOrDefault(r => r.RideId == rideId);
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

        if (!visitor.ActiveTicket.GrantsAccessToAllRides() &&
            !visitor.ActiveTicket.AllowedRideIds.Contains(rideId))
        {
            return (false, $"Access denied: Ticket does not include access to '{ride.Name}'.");
        }

        var eligibility = ride.CheckEligibility(visitor);
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
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == visitorId);
        if (visitor == null)
        {
            return (false, $"Reservation failed: Visitor '{visitorId}' not found.");
        }

        var ride = _rides.FirstOrDefault(r => r.RideId == rideId);
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

        bool alreadyReserved = _reservations.Any(r =>
            r.VisitorId == visitorId &&
            r.RideId == rideId &&
            r.TimeSlot == timeSlot &&
            r.Status == ReservationStatus.Active);

        if (alreadyReserved)
        {
            return (false, "Reservation failed: Visitor already has a reservation for this ride and time slot.");
        }

        int reservedCount = _reservations.Count(r =>
            r.RideId == rideId &&
            r.TimeSlot == timeSlot &&
            r.Status == ReservationStatus.Active);

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

        _reservations.Add(reservation);

        return (true, $"Reservation {reservation.ReservationId} created for '{ride.Name}' at {timeSlot}.");
    }

    public (bool Success, string Message) CancelReservation(string reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
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

        _rides.Add(ride);

        return (true, $"Ride '{ride.Name}' added successfully with ID {ride.RideId}.");
    }

    public (bool Success, string Message) UpdateRideStatus(string rideId, RideStatus newStatus)
    {
        var ride = _rides.FirstOrDefault(r => r.RideId == rideId);
        if (ride == null)
        {
            return (false, $"Update ride status failed: Ride '{rideId}' not found.");
        }

        ride.Status = newStatus;

        return (true, $"Ride '{ride.Name}' status updated to {newStatus}.");
    }

    public (bool Success, string Message) AssignEmployee(string employeeId, string rideOrFacilityId, Shift shift)
    {
        var employee = _employees.FirstOrDefault(e => e.EmployeeId == employeeId);
        if (employee == null)
        {
            return (false, $"Assignment failed: Employee '{employeeId}' not found.");
        }

        bool isValidRide = _rides.Any(r => r.RideId == rideOrFacilityId);
        bool isValidFacility = _knownFacilities.Contains(rideOrFacilityId);

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
        var ride = _rides.FirstOrDefault(r => r.RideId == rideId);
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

        _employees.Add(employee);

        return (true, $"Employee '{name}' registered successfully with ID {employeeId}.");
    }

public string GetInfo(EntitySector entitySector, string id)
{
    if (entitySector == EntitySector.Visitor)
    {
        var visitor = _visitors.FirstOrDefault(v => v.VisitorId == id);
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
               $"  Ticket:   {ticketInfo}";
    }

    if (entitySector == EntitySector.Ride)
    {
        var ride = _rides.FirstOrDefault(r => r.RideId == id);
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
        var employee = _employees.FirstOrDefault(e => e.EmployeeId == id);
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
        var ticket = _tickets.FirstOrDefault(t => t.TicketId == id);
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
        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == id);
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