using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data; 
using RaceDay.Api.DTOs; 
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")]
public class ResultsController(RaceDayContext db):ControllerBase {
    int CurrentId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 /// <summary>Records a result for an enrolment in an event owned by the Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpPost("enrolments/{enrolmentId:int}/result")]
    public async Task<IActionResult> Create(int enrolmentId,ResultRequest r)
    {
        var en=await db.EventEnrolments.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.EnrolmentID==enrolmentId);
        if(en is null)return NotFound();if(en.Event!.OrganiserID!=CurrentId)
            return Forbid();
        if(await db.Results.AnyAsync(x=>x.EnrolmentID==enrolmentId))
            return Conflict(new{message="A result already exists for this enrolment."});
        var result=new Result{EnrolmentID=enrolmentId,Position=r.Position,FinishTime=r.FinishTime,Score=r.Score};
        db.Results.Add(result);en.Status="Completed";
        await db.SaveChangesAsync();
        return Created($"/api/results/{result.ResultID}",result);
    }
 /// <summary>Lists public results for an event.</summary>
 [HttpGet("events/{eventId:int}/results")] 
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if(!await db.Events.AnyAsync(e=>e.EventID==eventId))
            return NotFound();
        return Ok(await db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Participant).Where(x=>x.Enrolment!.EventID==eventId).OrderBy(x=>x.Position).Select(x=>new{x.ResultID,x.EnrolmentID,x.Position,x.FinishTime,x.Score,ParticipantName=x.Enrolment!.Participant!.FirstName+" "+x.Enrolment.Participant.LastName}).ToListAsync());
    }
 /// <summary>Lists results belonging to the logged-in Participant.</summary>
 [Authorize(Roles="Participant"),HttpGet("results/me")] 
    public async Task<IActionResult> Mine()=>Ok(await db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Event).Where(x=>x.Enrolment!.ParticipantsID==CurrentId).Select(x=>new{x.ResultID,x.EnrolmentID,EventName=x.Enrolment!.Event!.EventName,x.Position,x.FinishTime,x.Score,x.RecordedAt}).ToListAsync());
 /// <summary>Gets result details. Participants may view their own; Organisers may view results for their own events.</summary> 
 [Authorize,HttpGet("results/{id:int}")] 
    public async Task<IActionResult> Get(int id)
    {
        var r=await db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Event).Include(x=>x.Enrolment).ThenInclude(x=>x!.Participant).SingleOrDefaultAsync(x=>x.ResultID==id);
        if(r is null)
            return NotFound();
        if(User.IsInRole("Participant")&&r.Enrolment!.ParticipantsID!=CurrentId)return Forbid();
        if(User.IsInRole("Organiser")&&r.Enrolment!.Event!.OrganiserID!=CurrentId)return Forbid();
        return Ok(new{r.ResultID,r.EnrolmentID,r.Position,r.FinishTime,r.Score,r.RecordedAt,EventName=r.Enrolment!.Event!.EventName,ParticipantName=r.Enrolment.Participant!.FirstName+" "+r.Enrolment.Participant.LastName});
    }
 /// <summary>Corrects a result for an event owned by the Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpPut("results/{id:int}")] 
    public async Task<IActionResult> Update(int id,ResultRequest request)
    {
        var r=await db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Event).SingleOrDefaultAsync(x=>x.ResultID==id);
        if(r is null)
            return NotFound();
        if(r.Enrolment!.Event!.OrganiserID!=CurrentId)
            return Forbid();
        r.Position=request.Position;
        r.FinishTime=request.FinishTime;
        r.Score=request.Score;await db.SaveChangesAsync();
        return Ok(r);
    }
 /// <summary>Deletes a result for an event owned by the Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpDelete("results/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r=await db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Event).SingleOrDefaultAsync(x=>x.ResultID==id);
        if(r is null)return NotFound();
        if(r.Enrolment!.Event!.OrganiserID!=CurrentId)return Forbid();
        db.Results.Remove(r);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
