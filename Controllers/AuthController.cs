using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data; 
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/auth")]
public class AuthController(RaceDayContext db, IPasswordHasher<User> hasher):ControllerBase {//start of class
 /// <summary>Registers an Organiser or Participant. Passwords are stored only as hashes.</summary>
 [HttpPost("register")] public async Task<IActionResult> Register(RegisterRequest r) { var roleName=r.RoleName.Trim(); if(roleName is not ("Organiser" or "Participant")) return BadRequest(new{message="RoleName must be Organiser or Participant."}); var email=r.Email.Trim().ToLowerInvariant(); if(await db.Users.AnyAsync(u=>u.Email==email)) return Conflict(new{message="Email already registered."}); var role=await db.Roles.SingleOrDefaultAsync(x=>x.RoleName==roleName); if(role is null) return StatusCode(500,new{message="Role seed data is missing."}); var u=new User{FirstName=r.FirstName.Trim(),LastName=r.LastName.Trim(),Email=email,Phone=r.Phone.Trim(),RoleID=role.RoleID}; u.PasswordHash=hasher.HashPassword(u,r.Password); db.Users.Add(u); await db.SaveChangesAsync(); return Created("/api/users/me",new{u.UserID,u.FirstName,u.LastName,u.Email,u.Phone,Role=roleName,u.CreatedAt}); }
 /// <summary>Checks credentials and establishes a server-side session cookie.</summary>
 [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest r) { var u=await db.Users.Include(x=>x.Role).SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant()); if(u is null || hasher.VerifyHashedPassword(u,u.PasswordHash,r.Password)==PasswordVerificationResult.Failed) return Unauthorized(new{message="Invalid email or password."}); HttpContext.Session.Clear(); HttpContext.Session.SetInt32("UserID",u.UserID); HttpContext.Session.SetString("RoleName",u.Role!.RoleName); HttpContext.Session.SetString("Email",u.Email); return Ok(new{message="Login successful",u.UserID,u.Email,Role=u.Role.RoleName,authentication="Session cookie"}); }
 /// <summary>Ends the current session.</summary>
 [HttpPost("logout")] public IActionResult Logout(){HttpContext.Session.Clear();return Ok(new{message="Logged out successfully."});}
}//end of class
