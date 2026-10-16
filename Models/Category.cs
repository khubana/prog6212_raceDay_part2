namespace RaceDay.Api.Models;
public class Category { public int CategoryID { get; set; } public int EventID { get; set; } public string CategoryName { get; set; } = ""; public int MaxParticipants { get; set; } public decimal EntryFee { get; set; } = 50m; public Event? Event { get; set; } public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>(); }
