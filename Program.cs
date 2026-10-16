using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Middleware;
using RaceDay.Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "RaceDay API", Version = "v1", Description = "RaceDay event management API. Login first to establish a session cookie." }); });
builder.Services.AddDbContext<RaceDayContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.Name = ".RaceDay.Session"; options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.Cookie.SameSite = SameSiteMode.Lax; options.IdleTimeout = TimeSpan.FromMinutes(30); });
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthorization();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseSession();
app.UseMiddleware<SessionAuthenticationMiddleware>();
app.UseAuthorization();
app.MapControllers();
// Avoid automatic database/file creation at startup. Attach or create the RaceDay database in SQL Server first.
// Seed Roles (Organiser and Participant) in your existing database before registering users.
app.Run();
public partial class Program { }
