using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Models;

namespace FitMindAI.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // Domain entities
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Member> Members => Set<Member>();
    
    // Junction tables
    public DbSet<TrainerSpecialty> TrainerSpecialties => Set<TrainerSpecialty>();
    public DbSet<TrainerService> TrainerServices => Set<TrainerService>();
    
    // Phase 2 entities (placeholder for now)
    public DbSet<TrainerAvailability> TrainerAvailabilities => Set<TrainerAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AiRecommendation> AiRecommendations => Set<AiRecommendation>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // TrainerSpecialty - Many-to-Many configuration
        modelBuilder.Entity<TrainerSpecialty>()
            .HasKey(ts => new { ts.TrainerId, ts.SpecialtyId });
            
        modelBuilder.Entity<TrainerSpecialty>()
            .HasOne(ts => ts.Trainer)
            .WithMany(t => t.TrainerSpecialties)
            .HasForeignKey(ts => ts.TrainerId);
            
        modelBuilder.Entity<TrainerSpecialty>()
            .HasOne(ts => ts.Specialty)
            .WithMany(s => s.TrainerSpecialties)
            .HasForeignKey(ts => ts.SpecialtyId);
        
        // TrainerService - Many-to-Many configuration
        modelBuilder.Entity<TrainerService>()
            .HasKey(ts => new { ts.TrainerId, ts.ServiceTypeId });
            
        modelBuilder.Entity<TrainerService>()
            .HasOne(ts => ts.Trainer)
            .WithMany(t => t.TrainerServices)
            .HasForeignKey(ts => ts.TrainerId);
            
        modelBuilder.Entity<TrainerService>()
            .HasOne(ts => ts.ServiceType)
            .WithMany(st => st.TrainerServices)
            .HasForeignKey(ts => ts.ServiceTypeId);
        
        // Member - Identity User relationship (One-to-One)
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.UserId)
            .IsUnique();
    }
}

