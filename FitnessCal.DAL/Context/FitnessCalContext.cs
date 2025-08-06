using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Context;

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
            entity.Property(e => e.GoalNote).HasColumnName("goal_note");
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
            entity.HasKey(e => e.MealItemId).HasName("PK__UserMeal__B4B7B7B7B7B7B7B7");
            entity.Property(e => e.FoodId).HasColumnName("food_id");
            entity.Property(e => e.MealItemId).HasColumnName("meal_item_id");
            entity.Property(e => e.MealLogId).HasColumnName("meal_log_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ServingSize).HasColumnName("serving_size");
        });

        modelBuilder.Entity<UserMealLog>(entity =>
        {
            entity.HasKey(e => e.MealLogId).HasName("PK__UserMeal__B4B7B7B7B7B7B7B7");
            entity.Property(e => e.MealLogId).HasColumnName("meal_log_id");
            entity.Property(e => e.MealDate).HasColumnName("meal_date");
            entity.Property(e => e.MealType).HasColumnName("meal_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
