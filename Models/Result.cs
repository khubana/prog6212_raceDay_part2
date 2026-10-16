namespace RaceDay.Api.Models;
public class Result { public int ResultID { get; set; } public int EnrolmentID { get; set; } public int Position { get; set; } public TimeSpan? FinishTime { get; set; } public decimal? Score { get; set; } public DateTime RecordedAt { get; set; } = DateTime.UtcNow; public EventEnrolment? Enrolment { get; set; } }
