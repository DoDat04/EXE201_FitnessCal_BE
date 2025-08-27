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
            entity.ToTable("foods");

            entity.HasKey(e => e.FoodId).HasName("foods_pkey");

            entity.Property(e => e.FoodId)
                .HasColumnName("foodid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("Name")
                .HasColumnType("character varying");

            entity.Property(e => e.Calories)
                .IsRequired()
                .HasColumnName("calories")
                .HasColumnType("double precision");

            entity.Property(e => e.Carbs)
                .IsRequired()
                .HasColumnName("carbs")
                .HasColumnType("double precision");

            entity.Property(e => e.Fat)
                .IsRequired()
                .HasColumnName("fat")
                .HasColumnType("double precision");

            entity.Property(e => e.Protein)
                .IsRequired()
                .HasColumnName("protein")
                .HasColumnType("double precision");

            entity.Property(e => e.FoodCategory)
                .HasColumnName("foodcategory")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<PredefinedDish>(entity =>
        {
            entity.ToTable("predefineddishes");

            entity.HasKey(e => e.DishId).HasName("predefineddishes_pkey");

            entity.Property(e => e.DishId)
                .HasColumnName("dishid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("Name")
                .HasColumnType("character varying");

            entity.Property(e => e.Calories)
                .IsRequired()
                .HasColumnName("calories")
                .HasColumnType("double precision");

            entity.Property(e => e.Carbs)
                .IsRequired()
                .HasColumnName("carbs")
                .HasColumnType("double precision");

            entity.Property(e => e.Fat)
                .IsRequired()
                .HasColumnName("fat")
                .HasColumnType("double precision");

            entity.Property(e => e.Protein)
                .IsRequired()
                .HasColumnName("protein")
                .HasColumnType("double precision");

            entity.Property(e => e.ServingUnit)
                .HasColumnName("servingunit")
                .HasColumnType("character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userid")
                .HasColumnType("uuid");

            entity.Property(e => e.FirstName)
                .HasColumnName("firstname")
                .HasColumnType("character varying");

            entity.Property(e => e.LastName)
                .HasColumnName("lastname")
                .HasColumnType("character varying");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email")
                .HasColumnType("character varying");

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasColumnName("passwordhash")
                .HasColumnType("text");

            entity.Property(e => e.Role)
                .IsRequired()
                .HasColumnName("role")
                .HasColumnType("character varying");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasColumnType("smallint");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<UserDailyIntake>(entity =>
        {
            entity.ToTable("userdailyintake");

            entity.HasKey(e => e.Id).HasName("userdailyintake_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("userid")
                .HasColumnType("uuid");

            entity.Property(e => e.MealDate)
                .IsRequired()
                .HasColumnName("mealdate")
                .HasColumnType("date");

            entity.Property(e => e.TotalCalories)
                .HasDefaultValue(0.0)
                .HasColumnName("totalcalories")
                .HasColumnType("double precision");

            entity.Property(e => e.TargetCalories)
                .IsRequired()
                .HasColumnName("targetcalories")
                .HasColumnType("double precision");

            entity.HasOne(d => d.User).WithMany(p => p.UserDailyIntakes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("userdailyintake_userid_fkey");
        });

        modelBuilder.Entity<UserHealth>(entity =>
        {
            entity.ToTable("userhealth");

            entity.HasKey(e => e.UserId).HasName("userhealth_pkey");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userid")
                .HasColumnType("uuid");

            entity.Property(e => e.Gender)
                .HasColumnName("gender")
                .HasColumnType("character varying");

            entity.Property(e => e.DateOfBirth)
                .HasColumnName("date_of_birth")
                .HasColumnType("date");

            entity.Property(e => e.HeightCm)
                .HasColumnName("height_cm")
                .HasColumnType("numeric");

            entity.Property(e => e.WeightKg)
                .HasColumnName("weight_kg")
                .HasColumnType("numeric");

            entity.Property(e => e.TargetWeightKg)
                .HasColumnName("target_weight_kg")
                .HasColumnType("numeric");

            entity.Property(e => e.ActivityLevel)
                .HasColumnName("activity_level")
                .HasColumnType("character varying");

            entity.Property(e => e.Goal)
                .HasColumnName("goal")
                .HasColumnType("character varying");

            entity.Property(e => e.DietType)
                .HasColumnName("diet_type")
                .HasColumnType("character varying");

            entity.Property(e => e.IntensityLevel)
                .HasColumnName("intensity_level")
                .HasColumnType("character varying");

            entity.Property(e => e.EstimatedGoalDate)
                .HasColumnName("estimated_goal_date")
                .HasColumnType("date");

            entity.Property(e => e.GoalNote)
                .HasColumnName("goal_note")
                .HasColumnType("character varying");

            entity.Property(e => e.DailyCalories)
                .HasColumnName("daily_calories")
                .HasColumnType("double precision");

            entity.HasOne(d => d.User).WithOne(p => p.UserHealth)
                .HasForeignKey<UserHealth>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userhealth_userid_fkey");
        });

        modelBuilder.Entity<UserMealItem>(entity =>
        {
            entity.ToTable("usermealitems");

            entity.HasKey(e => e.ItemId).HasName("usermealitems_pkey");

            entity.Property(e => e.ItemId)
                .HasColumnName("itemid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.LogId)
                .HasColumnName("logid");

            entity.Property(e => e.IsCustom)
                .IsRequired()
                .HasColumnName("iscustom")
                .HasColumnType("smallint");

            entity.Property(e => e.DishId)
                .HasColumnName("dishid");

            entity.Property(e => e.FoodId)
                .HasColumnName("foodid");

            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasColumnName("quantity")
                .HasColumnType("double precision");

            entity.Property(e => e.Calories)
                .HasColumnName("calories")
                .HasColumnType("double precision");

            entity.HasOne(d => d.Dish).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.DishId)
                .HasConstraintName("usermealitems_dishid_fkey");

            entity.HasOne(d => d.Food).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("usermealitems_foodid_fkey");

            entity.HasOne(d => d.Log).WithMany(p => p.UserMealItems)
                .HasForeignKey(d => d.LogId)
                .HasConstraintName("usermealitems_logid_fkey");
        });

        modelBuilder.Entity<UserMealLog>(entity =>
        {
            entity.ToTable("usermeallog");

            entity.HasKey(e => e.LogId).HasName("usermeallog_pkey");

            entity.Property(e => e.LogId)
                .HasColumnName("logid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("userid")
                .HasColumnType("uuid");

            entity.Property(e => e.MealDate)
                .IsRequired()
                .HasColumnName("mealdate")
                .HasColumnType("date");

            entity.Property(e => e.MealType)
                .HasColumnName("mealtype")
                .HasColumnType("character varying");

            entity.HasOne(d => d.User).WithMany(p => p.UserMealLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("usermeallog_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
