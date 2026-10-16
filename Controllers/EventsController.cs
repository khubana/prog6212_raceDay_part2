using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data; 
using RaceDay.Api.DTOs; 
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/events")] 
public class EventsController(RaceDayContext db):ControllerBase {//start of class
    int CurrentId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 /// <summary>Lists public events.</summary> 
 [HttpGet] 
    public async Task<IActionResult> All()=>Ok(await db.Events.AsNoTracking().Select(e=>new{e.EventID,e.EventName,e.EventDescription,e.EventDate,e.EventLocation,e.EventStatus,e.OrganiserID}).ToListAsync());
 /// <summary>Gets one event and its categories..</summary> 
 [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id){
        var e=await db.Events.AsNoTracking().Include(x=>x.Categories).Where(x=>x.EventID==id).Select(x=>new{x.EventID,x.EventName,x.EventDescription,x.EventDate,x.EventLocation,x.EventStatus,x.OrganiserID,Categories=x.Categories.Select(c=>new{c.CategoryID,c.CategoryName,c.MaxParticipants,c.EntryFee})}).SingleOrDefaultAsync();
        return e is null?NotFound():Ok(e);
    }
 /// <summary>Creates an event owned by the logged-in Organiser.</summary> 
 [Authorize(Roles="Organiser"),HttpPost] 
    public async Task<IActionResult> Create(EventRequest r){
        if(!Valid(r))return BadRequest(new{message="Invalid event status."});
        var e=Make(r,CurrentId);db.Events.Add(e);await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get),new{id=e.EventID},e);
    }
 /// <summary>Updates an event created by the logged-in Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpPut("{id:int}")] 
    public async Task<IActionResult> Update(int id,EventRequest r){
        var e=await db.Events.FindAsync(id);
        if(e is null)return NotFound();
        if(e.OrganiserID!=CurrentId)return Forbid();
        if(!Valid(r))return BadRequest(new{message="Invalid event status."});
        e.EventName=r.EventName;e.EventDescription=r.EventDescription;e.EventDate=r.EventDate;e.EventLocation=r.EventLocation;e.EventStatus=r.EventStatus.ToLowerInvariant();await db.SaveChangesAsync()
            ;return Ok(e);
    }
 /// <summary>Deletes an event owned by the logged-in Organiser.</summary> 
[Authorize(Roles="Organiser"),HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {var e=await db.Events.FindAsync(id);
        if(e is null)return NotFound();
        if(e.OrganiserID!=CurrentId)
            return Forbid();
        db.Events.Remove(e);
        await db.SaveChangesAsync();
        return NoContent();
    }
 bool Valid(EventRequest r)=>(new[]{"open","closed","cancelled","completed"}.Contains(r.EventStatus.ToLowerInvariant())); static Event Make(EventRequest r,int uid)=>new(){OrganiserID=uid,EventName=r.EventName.Trim(),EventDescription=r.EventDescription.Trim(),EventDate=r.EventDate.Date,EventLocation=r.EventLocation.Trim(),EventStatus=r.EventStatus.ToLowerInvariant()};
}
