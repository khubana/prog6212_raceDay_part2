using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.DTOs;
public record RegisterRequest([Required,MaxLength(100)] string FirstName,[Required,MaxLength(100)] string LastName,[Required,EmailAddress,MaxLength(100)] string Email,[Required,MinLength(8)] string Password,[Required,MaxLength(20)] string Phone,[Required] string RoleName);
public record LoginRequest([Required,EmailAddress] string Email,[Required] string Password);
public record UpdateProfileRequest([Required,MaxLength(100)] string FirstName,[Required,MaxLength(100)] string LastName,[Required,EmailAddress,MaxLength(100)] string Email,[Required,MaxLength(20)] string Phone);
public record EventRequest([Required,MaxLength(100)] string EventName,[Required,MaxLength(1000)] string EventDescription,[Required] DateTime EventDate,[Required,MaxLength(100)] string EventLocation,[Required] string EventStatus="open");
public record CategoryRequest([Required,MaxLength(100)] string CategoryName,[Range(10,100000)] int MaxParticipants,[Range(50,1000000)] decimal EntryFee);
public record EnrolmentRequest([Range(1,int.MaxValue)] int CategoryID);
public record ResultRequest([Range(1,int.MaxValue)] int Position,TimeSpan? FinishTime,[Range(0,1000000)] decimal? Score);
