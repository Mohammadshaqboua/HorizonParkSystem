/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Services;
using HorizonParkSystem.Models;
using HorizonParkSystem.Enums;

var parkService = new ParkSystemService();

while (true)
{
    Console.Clear();

    Console.WriteLine("=================================================");
    Console.WriteLine("       HORIZON ADVENTURE PARK");
    Console.WriteLine("          OPERATIONS SYSTEM");
    Console.WriteLine("=================================================");
    Console.WriteLine();
    Console.WriteLine("  [1]  Register Visitor");
    Console.WriteLine("  [2]  Issue Ticket");
    Console.WriteLine("  [3]  Deactivate Ticket");
    Console.WriteLine("  [4]  Validate Ticket");
    Console.WriteLine("  [5]  Validate Ride Access");
    Console.WriteLine("  [6]  Create Reservation");
    Console.WriteLine("  [7]  Cancel Reservation");
    Console.WriteLine("  [8]  Add Ride");
    Console.WriteLine("  [9]  Update Ride Status");
    Console.WriteLine("  [10] Assign Employee");
    Console.WriteLine("  [11] View Ride Occupancy Status");
    Console.WriteLine("  [12] Register Employee");
    Console.WriteLine("  [13] Get Info");
    Console.WriteLine("  [14] Display All Data");
    Console.WriteLine("  [15] Exit");
    Console.WriteLine();
    Console.WriteLine("-------------------------------------------------");
    Console.Write("Select an option: ");

    string input = Console.ReadLine();

    if (!int.TryParse(input, out int choice))
    {
        Console.WriteLine();
        Console.WriteLine("[ERROR] Invalid input. Please enter a number.");
        Console.WriteLine();
        Console.Write("Press ENTER to continue...");
        Console.ReadLine();
        continue;
    }

    switch (choice)
    {
        case 1:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("              REGISTER VISITOR");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Name cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Age: ");

            if (!int.TryParse(Console.ReadLine(), out int age))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid age.");
                Console.WriteLine("[INFO] Registration cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Height (cm): ");

            if (!int.TryParse(Console.ReadLine(), out int heightCm))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid height.");
                Console.WriteLine("[INFO] Registration cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Visitor Category:");
            Console.WriteLine("  [1] General");
            Console.WriteLine("  [2] VIP");
            Console.WriteLine("  [3] Child");
            Console.WriteLine("  [4] Senior");
            Console.WriteLine("  [5] Staff Accompanied Minor");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int catChoice) ||
                catChoice < 1 ||
                catChoice > 5)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Registration cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            VisitorCategory category = catChoice switch
            {
                1 => VisitorCategory.General,
                2 => VisitorCategory.VIP,
                3 => VisitorCategory.Child,
                4 => VisitorCategory.Senior,
                5 => VisitorCategory.StaffAccompaniedMinor,
                _ => VisitorCategory.General
            };

            bool hasAccompanyingAdult = false;
            
            if (category == VisitorCategory.Child)
            {
                
                Console.WriteLine();
                Console.Write("Is the child accompanied by an adult? (y/n): ");
                string accompaniedAnswer = Console.ReadLine();
                
                if(accompaniedAnswer.Trim().ToLower() == "y")
                    hasAccompanyingAdult = true;
                else
                {
                    hasAccompanyingAdult = false;
                }

            }
            
            var result = parkService.RegisterVisitor(
                name,
                age,
                heightCm,
                category,
                hasAccompanyingAdult
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 2:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("                ISSUE TICKET");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Visitor ID: ");
            string visitorId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Visitor Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Ticket Type:");
            Console.WriteLine("  [1] Regular");
            Console.WriteLine("  [2] VIP");
            Console.WriteLine("  [3] Child");
            Console.WriteLine("  [4] Senior");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int typeChoice) ||
                typeChoice < 1 ||
                typeChoice > 4)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Ticket issuance cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            TicketType type = typeChoice switch
            {
                1 => TicketType.Regular,
                2 => TicketType.VIP,
                3 => TicketType.Child,
                4 => TicketType.Senior,
                _ => TicketType.Regular
            };

            string[] allowedRideIds = new string[0];

            if (type != TicketType.VIP)
            {
                Console.WriteLine();
                Console.Write("Enter Allowed Ride IDs (e.g. RIDE-1,RIDE-2): ");

                string ridesInput = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(ridesInput))
                {
                    string[] rawParts = ridesInput.Split(',');

                    foreach (var part in rawParts)
                    {
                        string trimmed = part.Trim();

                        Array.Resize(ref allowedRideIds, allowedRideIds.Length + 1);
                        allowedRideIds[allowedRideIds.Length - 1] = trimmed;
                    }
                }
            }
            
            var result = parkService.IssueTicket(
                visitorId,
                type,
                allowedRideIds
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 3:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("             DEACTIVATE TICKET");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Visitor ID: ");
            string visitorId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Visitor Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            var result = parkService.DeactivateTicket(visitorId);

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 4:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("               VALIDATE TICKET");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Visitor ID: ");
            string visitorId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Visitor Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            var result = parkService.ValidateTicket(visitorId);

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 5:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("            VALIDATE RIDE ACCESS");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Visitor ID: ");
            string visitorId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Visitor ID cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Ride ID: ");
            string rideId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rideId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Ride ID cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }
            
            var visitor = parkService.GetVisitor(visitorId);
            if (visitor == null)
            {
                Console.WriteLine();
                Console.WriteLine($"[ERROR] Visitor '{visitorId}' not found.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            bool hasAccompanyingAdult = visitor.HasAccompanyingAdult;

            var result = parkService.CheckRideAccess(
                visitorId,
                rideId,
                hasAccompanyingAdult
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 6:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("            CREATE RESERVATION");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Visitor ID: ");
            string visitorId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Visitor ID cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Ride ID: ");
            string rideId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rideId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Ride ID cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Time Slot: ");
            string timeSlot = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(timeSlot))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Time Slot cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            var result = parkService.CreateReservation(
                visitorId,
                rideId,
                timeSlot
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 7:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("            CANCEL RESERVATION");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Reservation ID: ");
            string reservationId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(reservationId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Reservation Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            var result = parkService.CancelReservation(
                reservationId
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 8:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("                  ADD RIDE");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Ride Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Name cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Ride Type:");
            Console.WriteLine("  [1] Thrill");
            Console.WriteLine("  [2] Family");
            Console.WriteLine("  [3] Water");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int typeChoice)||
                typeChoice < 1 ||
                typeChoice > 3)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            RideType type = typeChoice switch
            {
                1 => RideType.Thrill,
                2 => RideType.Family,
                3 => RideType.Water,
                _ => RideType.Family
            };

            Console.Write("Enter Min Age: ");

            if (!int.TryParse(Console.ReadLine(), out int minAge))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid age.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Min Height (cm): ");

            if (!int.TryParse(Console.ReadLine(), out int minHeightCm))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid height.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Requires Accompanying Adult (true/false): ");

            if (!bool.TryParse(Console.ReadLine(), out bool requiresAdult))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Max Capacity: ");

            if (!int.TryParse(Console.ReadLine(), out int maxCapacity))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid capacity.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Ride Status:");
            Console.WriteLine("  [1] Open");
            Console.WriteLine("  [2] Closed");
            Console.WriteLine("  [3] Under Maintenance");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int statusChoice)||
                typeChoice < 1 ||
                typeChoice > 3)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Ride creation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            RideStatus status = statusChoice switch
            {
                1 => RideStatus.Open,
                2 => RideStatus.Closed,
                3 => RideStatus.UnderMaintenance,
                _ => RideStatus.Open
            };

            var newRide = new Ride
            {
                Name = name,
                Type = type,
                MinAge = minAge,
                MinHeightCm = minHeightCm,
                RequiresAccompanyingAdult = requiresAdult,
                MaxCapacity = maxCapacity,
                CurrentOccupancy = 0,
                Status = status
            };

            var result = parkService.AddRide(newRide);

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 9:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("             UPDATE RIDE STATUS");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Ride ID: ");
            string rideId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rideId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Ride Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Ride Status:");
            Console.WriteLine("  [1] Open");
            Console.WriteLine("  [2] Closed");
            Console.WriteLine("  [3] Under Maintenance");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int statusChoice)||
                statusChoice < 1 ||
                statusChoice > 3)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Update cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            RideStatus status = statusChoice switch
            {
                1 => RideStatus.Open,
                2 => RideStatus.Closed,
                3 => RideStatus.UnderMaintenance,
                _ => RideStatus.Open
            };

            var result = parkService.UpdateRideStatus(
                rideId,
                status
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 10:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("              ASSIGN EMPLOYEE");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Employee ID: ");
            string employeeId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Employee Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.Write("Enter Ride Or Facility ID: ");
            string rideOrFacilityId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rideOrFacilityId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Ride Or Facility Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Shift:");
            Console.WriteLine("  [1] Morning");
            Console.WriteLine("  [2] Afternoon");
            Console.WriteLine("  [3] Evening");
            Console.WriteLine("  [4] Night");
            Console.WriteLine();
            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int shiftChoice)||
                shiftChoice < 1 ||
                shiftChoice > 4)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Assignment cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Shift shift = shiftChoice switch
            {
                1 => Shift.Morning,
                2 => Shift.Afternoon,
                3 => Shift.Evening,
                4 => Shift.Night,
                _ => Shift.Morning
            };

            var result = parkService.AssignEmployee(
                employeeId,
                rideOrFacilityId,
                shift
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 11:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("          RIDE OCCUPANCY STATUS");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Ride ID: ");
            string rideId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rideId))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Ride Id cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            var result = parkService.GetRideOccupancyStatus(
                rideId
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
        
        case 12:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("             REGISTER EMPLOYEE");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Name cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine();
            Console.WriteLine("Select Role:");
            Console.WriteLine("  [1] Ticket Booth Staff");
            Console.WriteLine("  [2] Ride Operator");
            Console.WriteLine("  [3] Operations Manager");
            Console.WriteLine("  [4] Maintenance");
            Console.WriteLine();
            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int roleChoice)||
                roleChoice < 1 ||
                roleChoice > 4)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Registration cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            Role role = roleChoice switch
            {
                1 => Role.TicketBoothStaff,
                2 => Role.RideOperator,
                3 => Role.OperationsManager,
                4 => Role.Maintenance,
                _ => Role.TicketBoothStaff
            };

            var result = parkService.RegisterEmployee(
                name,
                role
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }

        case 13:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("                    GET INFO");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            Console.Write("Enter ID: ");
            string id = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] ID cannot be empty.");
                Console.WriteLine("[INFO] Operation cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("Select Entity Sector:");
            Console.WriteLine("  [1] Visitor");
            Console.WriteLine("  [2] Ride");
            Console.WriteLine("  [3] Employee");
            Console.WriteLine("  [4] Ticket");
            Console.WriteLine("  [5] Reservation");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int entitySector)||
                entitySector < 1 ||
                entitySector > 5)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Get Info cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            EntitySector sector = entitySector switch
            {
                1 => EntitySector.Visitor,
                2 => EntitySector.Ride,
                3 => EntitySector.Employee,
                4 => EntitySector.Ticket,
                5 => EntitySector.Reservation,
                _ => EntitySector.Visitor
            };

            var result = parkService.GetInfo(
                sector,
                id
            );

            Console.WriteLine();
            Console.WriteLine("[RESULT]");
            Console.WriteLine(result);
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }

        case 14:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("            DISPLAY ALL DATA");
            Console.WriteLine("=================================================");
            Console.WriteLine();
            
            Console.WriteLine("Select Entity Sector:");
            Console.WriteLine("  [1] Visitor");
            Console.WriteLine("  [2] Ride");
            Console.WriteLine("  [3] Employee");
            Console.WriteLine("  [4] Ticket");
            Console.WriteLine("  [5] Reservation");
            Console.WriteLine();

            Console.Write("Choose: ");

            if (!int.TryParse(Console.ReadLine(), out int entitySector)||
                entitySector < 1 ||
                entitySector > 5)
            {
                Console.WriteLine();
                Console.WriteLine("[ERROR] Invalid input.");
                Console.WriteLine("[INFO] Get Info cancelled.");
                Console.WriteLine();
                Console.Write("Press ENTER to continue...");
                Console.ReadLine();
                break;
            }

            EntitySector sector = entitySector switch
            {
                1 => EntitySector.Visitor,
                2 => EntitySector.Ride,
                3 => EntitySector.Employee,
                4 => EntitySector.Ticket,
                5 => EntitySector.Reservation,
                _ => EntitySector.Visitor
            };
            
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine($"          [ALL DATA IN {sector}]");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            parkService.DisplayAllData(sector);
            
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();
            
            break;
        }
        
        case 15:
        {
            Console.Clear();

            Console.WriteLine("=================================================");
            Console.WriteLine("          HORIZON ADVENTURE PARK");
            Console.WriteLine("=================================================");
            Console.WriteLine();
            Console.WriteLine("Exiting system. Goodbye!");
            Console.WriteLine();

            return;
        }

        default:
        {
            Console.WriteLine();
            Console.WriteLine("[ERROR] Invalid option.");
            Console.WriteLine("[INFO] Please select an option from 1 to 13.");
            Console.WriteLine();

            Console.Write("Press ENTER to continue...");
            Console.ReadLine();

            break;
        }
    }
}