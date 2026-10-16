using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using RaceDay.Api.Data; 
using RaceDay.Api.DTOs; 
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")] 
public class EnrolmentsController(RaceDayContext db):ControllerBase {//start of class
    int CurrentId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 /// <summary>Enrols the logged-in Participant in an event category..</summary>
 [Authorize(Roles="Participant"),HttpPost("events/{eventId:int}/enrolments")] 
    public async Task<IActionResult> Enrol(int eventId,EnrolmentRequest r){
        var e=await db.Events.FindAsync(eventId);
        if(e is null)return NotFound(new{message="Event not found."});
        if(e.EventStatus!="open"||e.EventDate.Date<DateTime.UtcNow.Date)
            return BadRequest(new{message="Event is not open for enrolment."});
        var c=await db.Categories.SingleOrDefaultAsync(x=>x.CategoryID==r.CategoryID&&x.EventID==eventId);
        if(c is null)return BadRequest(new{message="Category does not belong to this event."});
        if(await db.EventEnrolments.AnyAsync(x=>x.EventID==eventId&&x.ParticipantsID==CurrentId))
            return Conflict(new{message="Already enrolled in this event."});
        if(await db.EventEnrolments.CountAsync(x=>x.CategoryID==c.CategoryID&&x.Status=="Active")>=c.MaxParticipants)return Conflict(new{message="Category is full."});
        var en=new EventEnrolment{EventID=eventId,ParticipantsID=CurrentId,CategoryID=c.CategoryID};
        db.EventEnrolments.Add(en);
        await db.SaveChangesAsync();
        return Created($"/api/enrolments/{en.EnrolmentID}",new{en.EnrolmentID,en.EventID,en.ParticipantsID,en.CategoryID,en.EnrolmentDate,en.Status});}
 /// <summary>Lists enrolments for the authenticated Participant..</summary> 
 [Authorize(Roles="Participant"),HttpGet("enrolments/me")] 
    public async Task<IActionResult> Mine()=>Ok(await db.EventEnrolments.Include(x=>x.Event).Include(x=>x.Category).Where(x=>x.ParticipantsID==CurrentId).Select(x=>new{x.EnrolmentID,x.EventID,EventName=x.Event!.EventName,x.CategoryID,CategoryName=x.Category!.CategoryName,x.EnrolmentDate,x.Status}).ToListAsync());
 /// <summary>Lists enrolments for an event owned by the logged-in Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpGet("events/{eventId:int}/enrolments")] 
    public async Task<IActionResult> ForEvent(int eventId){
        var e=await db.Events.FindAsync(eventId);
        if(e is null)return NotFound();
        if(e.OrganiserID!=CurrentId)return Forbid();
        return Ok(await db.EventEnrolments.Include(x=>x.Participant).Include(x=>x.Category).Where(x=>x.EventID==eventId).Select(x=>new{x.EnrolmentID,x.EventID,ParticipantName=x.Participant!.FirstName+" "+x.Participant.LastName,x.Participant.Email,x.CategoryID,CategoryName=x.Category!.CategoryName,x.Status,x.EnrolmentDate}).ToListAsync());
    }
 /// <summary>Gets an enrolment owned by the Participant or for an event owned by the Organiser..</summary> 
[Authorize,HttpGet("enrolments/{id:int}")]
    public async Task<IActionResult> Get(int id){
        var en=await db.EventEnrolments.Include(x=>x.Event).Include(x=>x.Participant).Include(x=>x.Category).SingleOrDefaultAsync(x=>x.EnrolmentID==id);
        if(en is null)return NotFound();
        if(User.IsInRole("Participant")&&en.ParticipantsID!=CurrentId)
            return Forbid();
        if(User.IsInRole("Organiser")&&en.Event!.OrganiserID!=CurrentId)
            return Forbid();
        return Ok(new{en.EnrolmentID,en.EventID,EventName=en.Event!.EventName,en.ParticipantsID,ParticipantName=en.Participant!.FirstName+" "+en.Participant.LastName,en.CategoryID,CategoryName=en.Category!.CategoryName,en.EnrolmentDate,en.Status});
    }
 /// <summary>Cancels the authenticated Participant's own enrolment.</summary>
 [Authorize(Roles="Participant"),HttpDelete("enrolments/{id:int}")] 
    public async Task<IActionResult> Cancel(int id){
        var en=await db.EventEnrolments.FindAsync(id);
        if(en is null)return NotFound();
        if(en.ParticipantsID!=CurrentId)return Forbid();
        en.Status="Cancelled";await db.SaveChangesAsync();return NoContent();
    }
}
