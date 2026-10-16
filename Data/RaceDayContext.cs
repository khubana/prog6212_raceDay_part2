using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public class RaceDayContext(DbContextOptions<RaceDayContext> options) : DbContext(options) {
 public DbSet<Role> Roles => Set<Role>(); public DbSet<User> Users => Set<User>(); public DbSet<Event> Events => Set<Event>(); public DbSet<Category> Categories => Set<Category>(); public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>(); public DbSet<Result> Results => Set<Result>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<Role>().HasKey(x=>x.RoleID); b.Entity<Role>().HasIndex(x=>x.RoleName).IsUnique(); b.Entity<Role>().Property(x=>x.RoleName).HasMaxLength(100).IsRequired();
  b.Entity<User>().HasKey(x=>x.UserID); b.Entity<User>().HasIndex(x=>x.Email).IsUnique(); b.Entity<User>().Property(x=>x.Email).HasMaxLength(100).IsRequired(); b.Entity<User>().Property(x=>x.PasswordHash).HasMaxLength(500).IsRequired(); b.Entity<User>().Property(x=>x.Phone).HasMaxLength(20).IsRequired(); b.Entity<User>().HasOne(x=>x.Role).WithMany(x=>x.Users).HasForeignKey(x=>x.RoleID);
  b.Entity<Event>().HasKey(x=>x.EventID); b.Entity<Event>().Property(x=>x.EventName).HasMaxLength(100).IsRequired(); b.Entity<Event>().Property(x=>x.EventDescription).HasMaxLength(1000).IsRequired(); b.Entity<Event>().Property(x=>x.EventLocation).HasMaxLength(100).IsRequired(); b.Entity<Event>().Property(x=>x.EventStatus).HasMaxLength(20).HasDefaultValue("open"); b.Entity<Event>().HasOne(x=>x.Organiser).WithMany(x=>x.Events).HasForeignKey(x=>x.OrganiserID).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Category>().HasKey(x=>x.CategoryID); b.Entity<Category>().Property(x=>x.EntryFee).HasPrecision(10,2); b.Entity<Category>().HasOne(x=>x.Event).WithMany(x=>x.Categories).HasForeignKey(x=>x.EventID).OnDelete(DeleteBehavior.Cascade);
  b.Entity<EventEnrolment>().HasKey(x=>x.EnrolmentID); b.Entity<EventEnrolment>().HasIndex(x=>new{x.ParticipantsID,x.EventID}).IsUnique(); b.Entity<EventEnrolment>().HasOne(x=>x.Event).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.EventID).OnDelete(DeleteBehavior.Cascade); b.Entity<EventEnrolment>().HasOne(x=>x.Participant).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.ParticipantsID).OnDelete(DeleteBehavior.Restrict); b.Entity<EventEnrolment>().HasOne(x=>x.Category).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.CategoryID).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Result>().HasKey(x=>x.ResultID); b.Entity<Result>().HasIndex(x=>x.EnrolmentID).IsUnique(); b.Entity<Result>().Property(x=>x.Score).HasPrecision(10,2); b.Entity<Result>().HasOne(x=>x.Enrolment).WithOne(x=>x.Result).HasForeignKey<Result>(x=>x.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
 }
}
