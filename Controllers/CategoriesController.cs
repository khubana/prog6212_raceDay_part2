using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")] 
public class CategoriesController(RaceDayContext db):ControllerBase {
    int CurrentId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 /// <summary>Lists all categories.</summary> 
[HttpGet("categories")] 
    public async Task<IActionResult> All()=>Ok(await db.Categories.AsNoTracking().Select(c=>new{c.CategoryID,c.EventID,c.CategoryName,c.MaxParticipants,c.EntryFee}).ToListAsync());
 /// <summary>Lists categories for a specific event.</summary> 
 [HttpGet("events/{eventId:int}/categories")] 
    public async Task<IActionResult> ForEvent(int eventId){
        if(!await db.Events.AnyAsync(e=>e.EventID==eventId))
            return NotFound();
        return Ok(await db.Categories.Where(c=>c.EventID==eventId).Select(c=>new{c.CategoryID,c.EventID,c.CategoryName,c.MaxParticipants,c.EntryFee}).ToListAsync());
    }
 /// <summary>Creates a category for an event owned by the Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpPost("events/{eventId:int}/categories")]
        public async Task<IActionResult> Create(int eventId,CategoryRequest r){
        var e=await db.Events.FindAsync(eventId);
        if(e is null)return NotFound();
        if(e.OrganiserID!=CurrentId)return Forbid();
        var c=new Category{EventID=eventId,CategoryName=r.CategoryName.Trim(),MaxParticipants=r.MaxParticipants,EntryFee=r.EntryFee};
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/categories",c);
    }
 /// <summary>Updates a category belonging to an event owned by the Organiser.</summary>
 [Authorize(Roles="Organiser"),HttpPut("categories/{id:int}")]
    public async Task<IActionResult> Update(int id,CategoryRequest r){
        var c=await db.Categories.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.CategoryID==id);
        if(c is null)return NotFound();
        if(c.Event!.OrganiserID!=CurrentId)return Forbid();c.CategoryName=r.CategoryName.Trim();
        c.MaxParticipants=r.MaxParticipants;c.EntryFee=r.EntryFee;
        await db.SaveChangesAsync();return Ok(c);
    }
 /// <summary>Deletes a category belonging to an event owned by the Organiser.</summary> 
 [Authorize(Roles="Organiser"),HttpDelete("categories/{id:int}")] 
    public async Task<IActionResult> Delete(int id){
        var c=await db.Categories.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.CategoryID==id);
        if(c is null)return NotFound();
        if(c.Event!.OrganiserID!=CurrentId)return Forbid();
        if(await db.EventEnrolments.AnyAsync(x=>x.CategoryID==id))
            return Conflict(new{message="Cannot delete a category with enrolments."});
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
