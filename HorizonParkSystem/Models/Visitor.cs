/* ============================================================
   HorizonParkSystem - Park Management System
   Author: Mohammad Shaqboua
   GITHUB: https://github.com/Mohammadshaqboua/HorizonParkSystem.git
   ============================================================ 
*/

using HorizonParkSystem.Enums;

namespace HorizonParkSystem.Models;

public class Visitor
{
    public string          VisitorId    { get; set; }
    public string          Name         { get; set; }
    public int             Age          { get; set; }
    public int             HeightCm     { get; set; }
    public VisitorCategory Category     { get; set; }
    public Ticket          ActiveTicket { get; set; }   

    public Visitor(string visitorId, string name, int age, int heightCm, VisitorCategory category)
    {
        VisitorId = visitorId;
        Name = name;
        Age = age;
        HeightCm = heightCm;
        Category = category;
    }
}