using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data; 
using RaceDay.Api.DTOs;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/users"),Authorize]
public class UsersController(RaceDayContext db):ControllerBase {
    int CurrentId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 /// <summary>Returns the authenticated user's own profile.</summary>
 [HttpGet("me")] public async Task<IActionResult> Me()
    {
        var u=await db.Users.Include(x=>x.Role).Where(x=>x.UserID==CurrentId).Select(x=>new{x.UserID,x.FirstName,x.LastName,x.Email,x.Phone,Role=x.Role!.RoleName,x.CreatedAt}).SingleOrDefaultAsync();
        return u is null?NotFound():Ok(u);
    }
 /// <summary>Updates only the authenticated user's profile.</summary>
 [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateProfileRequest r)
    {
        var u=await db.Users.FindAsync(CurrentId);
        if(u is null)return NotFound();
        var email=r.Email.Trim().ToLowerInvariant();
        if(await db.Users.AnyAsync(x=>x.Email==email&&x.UserID!=CurrentId))
            return Conflict(new{message="Email already registered."});
        u.FirstName=r.FirstName.Trim();
        u.LastName=r.LastName.Trim();
        u.Email=email;u.Phone=r.Phone.Trim();await db.SaveChangesAsync();
        HttpContext.Session.SetString("Email",u.Email);
        return Ok(new{message="Profile updated",u.UserID,u.FirstName,u.LastName,u.Email,u.Phone});
    }
}
