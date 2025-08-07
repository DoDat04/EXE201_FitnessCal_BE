using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.Domain;

public partial class FitnessCalContext : DbContext
{
    public FitnessCalContext()
    {
    }

    public FitnessCalContext(DbContextOptions<FitnessCalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<PredefinedDish> PredefinedDishes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDailyIntake> UserDailyIntakes { get; set; }

    public virtual DbSet<UserHealth> UserHealths { get; set; }

    public virtual DbSet<UserMealItem> UserMealItems { get; set; }

    public virtual DbSet<UserMealLog> UserMealLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__Foods__856DB3EBD50CF461");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PredefinedDish>(entity =>
        {
            entity.HasKey(e => e.DishId).HasName("PK__Predefin__18834F509BB445B4");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ServingUnit).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CFFA00B4B3E");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164CBF40468").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("firstName");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("lastName");
            entity.Property(e => e.PasswordHash).HasColumnName("passwordHash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserDailyIntake>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserDail__3214EC073B8B9395");

            entity.ToTable("UserDailyIntake");

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalCalories).HasDefaultValue(0.0);

            entity.HasOne(d => d.User).WithMany(p => p.UserDailyIntakes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserDaily__UserI__6FE99F9F");
        });

        modelBuilder.Entity<UserHealth>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserHeal__CB9A1CFFD2662413");

            entity.ToTable("UserHealth");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userId");
            entity.Property(e => e.ActivityLevel)
                .HasMaxLength(20)
                .HasColumnName("activity_level");
            entity.Property(e => e.DailyCalories).HasColumnName("daily_calories");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DietType)
                .HasMaxLength(30)
                .HasColumnName("diet_type");
            entity.Property(e => e.EstimatedGoalDate).HasColumnName("estimated_goal_date");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Goal)
                .HasMaxLength(20)
                .HasColumnName("goal");
            entity.Property(e => e.GoalNote)
                .HasMaxLength(50)
                .HasColumnName("goal_note");
            entity.Property(e => e.HeightCm)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("height_cm");
            entity.Property(e => e.IntensityLevel)
                .HasMaxLength(20)
                .HasColumnName("intensity_level");
            entity.Property(e => e.TargetWeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("target_weight_kg");
            entity.Property(e => e.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.User).WithOne(p => p.UserHealth)
                .HasForeignKey<UserHealth>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserHealt__userI__4D94879B");
        });

        modelBuilder.Entity<UserMealItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__UserMeal__727E838BCB2D3C87");

            entity.HasOne(d => d.Dish).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.DishId)
                .HasConstraintName("FK__UserMealI__DishI__66603565");

            entity.HasOne(d => d.Food).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("FK__UserMealI__FoodI__6754599E");

            entity.HasOne(d => d.Log).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.LogId)
                .HasConstraintName("FK__UserMealI__LogId__656C112C");
        });

        modelBuilder.Entity<UserMealLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__UserMeal__5E548648EC0A39B8");

            entity.ToTable("UserMealLog");

            entity.Property(e => e.MealType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserMealLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserMealL__UserI__628FA481");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
