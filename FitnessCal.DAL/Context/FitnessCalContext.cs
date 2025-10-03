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

    public virtual DbSet<Allergy> Allergies { get; set; }

    public virtual DbSet<FavoriteFood> FavoriteFoods { get; set; }

    public virtual DbSet<UserWeightLog> UserWeightLogs { get; set; }

    public virtual DbSet<PremiumPackage> PremiumPackages { get; set; }

    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<OTP> OTPs { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<UserActivity> UserActivities { get; set; }

    public virtual DbSet<PackageFeature> PackageFeatures { get; set; }

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

            entity.Property(e => e.SupabaseUserId)
                .HasColumnName("supabase_user_id")
                .HasColumnType("character varying");
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

        modelBuilder.Entity<UserWeightLog>(entity =>
        {
            entity.ToTable("user_weight_logs");

            entity.HasKey(e => e.Id).HasName("user_weight_logs_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            entity.Property(e => e.WeightKg)
                .HasColumnName("weight_kg")
                .HasColumnType("numeric");

            entity.Property(e => e.LogDate)
                .HasColumnName("log_date")
                .HasColumnType("date");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_weight_logs_user_id_fkey");
        });


        modelBuilder.Entity<PremiumPackage>(entity =>
        {
            entity.ToTable("premium_packages");
            entity.HasKey(e => e.PackageId).HasName("premium_packages_pkey");

            entity.Property(e => e.PackageId)
                .HasColumnName("package_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasColumnType("character varying");

            entity.Property(e => e.DurationMonths)
                .HasColumnName("duration_months")
                .HasColumnType("numeric(5,2)");

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasColumnType("numeric");
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasKey(e => e.SubscriptionId).HasName("user_subscriptions_pkey");

            entity.Property(e => e.SubscriptionId)
                .HasColumnName("subscription_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            entity.Property(e => e.PackageId)
                .HasColumnName("package_id");

            entity.Property(e => e.PriceAtPurchase)
                .HasColumnName("price_at_purchase")
                .HasColumnType("numeric");

            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date");

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");

            entity.Property(e => e.PaymentStatus)
                .HasColumnName("payment_status")
                .HasColumnType("character varying");

            entity.HasOne(d => d.User).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_us_user");

            entity.HasOne(d => d.Package).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("fk_us_package");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.Property(e => e.PaymentId)
                .HasColumnName("payment_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.SubscriptionId)
                .HasColumnName("subscription_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric");

            entity.Property(e => e.PayosOrderCode)
                .HasColumnName("payos_order_code")
                .HasColumnType("character varying");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("character varying");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.PaidAt)
                .HasColumnName("paid_at")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("payments_subscription_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("payments_user_id_fkey");
        });

        modelBuilder.Entity<OTP>(entity =>
        {
            entity.ToTable("otps");
            entity.HasKey(e => e.Id).HasName("otps_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email")
                .HasColumnType("character varying");

            entity.Property(e => e.OTPCode)
                .IsRequired()
                .HasColumnName("otp")
                .HasColumnType("character varying");

            entity.Property(e => e.Purpose)
                .IsRequired()
                .HasColumnName("purpose")
                .HasColumnType("character varying");

            entity.Property(e => e.ExpiresAt)
                .IsRequired()
                .HasColumnName("expiresat")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.IsUsed)
                .HasColumnName("isused")
                .HasColumnType("boolean")
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("createdat")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.UsedAt)
                .HasColumnName("usedat")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Allergy>(entity =>
        {
            entity.ToTable("allergies");

            entity.HasKey(e => e.AllergyId).HasName("allergies_pkey");

            entity.Property(e => e.AllergyId)
                .HasColumnName("allergy_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            entity.Property(e => e.FoodId)
                .HasColumnName("food_id");

            entity.Property(e => e.DishId)
                .HasColumnName("dish_id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.User).WithMany(p => p.Allergies)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("allergies_user_id_fkey");

            entity.HasOne(d => d.Food).WithMany()
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("allergies_food_id_fkey");

            entity.HasOne(d => d.Dish).WithMany()
                .HasForeignKey(d => d.DishId)
                .HasConstraintName("allergies_dish_id_fkey");

            entity.ToTable(t => t.HasCheckConstraint("ck_allergy_one_of", "(food_id IS NOT NULL AND dish_id IS NULL) OR (food_id IS NULL AND dish_id IS NOT NULL)"));

            entity.HasIndex(e => new { e.UserId, e.FoodId })
                .IsUnique()
                .HasFilter("food_id IS NOT NULL")
                .HasDatabaseName("allergies_user_food_unique");

            entity.HasIndex(e => new { e.UserId, e.DishId })
                .IsUnique()
                .HasFilter("dish_id IS NOT NULL")
                .HasDatabaseName("allergies_user_dish_unique");
        });

        modelBuilder.Entity<FavoriteFood>(entity =>
        {
            entity.ToTable("favorite_foods");

            entity.HasKey(e => e.FavoriteId).HasName("favorite_foods_pkey");

            entity.Property(e => e.FavoriteId)
                .HasColumnName("favorite_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            entity.Property(e => e.FoodId)
                .HasColumnName("food_id");

            entity.Property(e => e.DishId)
                .HasColumnName("dish_id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteFoods)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorite_foods_user_id_fkey");

            entity.HasOne(d => d.Food).WithMany()
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("favorite_foods_food_id_fkey");

            entity.HasOne(d => d.PredefinedDish).WithMany()
                .HasForeignKey(d => d.DishId)
                .HasConstraintName("favorite_foods_dish_id_fkey");

            // Ràng buộc one-of: chỉ 1 trong 2 có giá trị
            entity.ToTable(t => t.HasCheckConstraint("ck_fav_one_of", "(food_id IS NOT NULL AND dish_id IS NULL) OR (food_id IS NULL AND dish_id IS NOT NULL)"));

            // Unique theo từng loại
            entity.HasIndex(e => new { e.UserId, e.FoodId })
                .IsUnique()
                .HasFilter("food_id IS NOT NULL")
                .HasDatabaseName("favorite_foods_user_food_unique");

            entity.HasIndex(e => new { e.UserId, e.DishId })
                .IsUnique()
                .HasFilter("dish_id IS NOT NULL")
                .HasDatabaseName("favorite_foods_user_dish_unique");
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("activities");

            entity.HasKey(e => e.ActivityId).HasName("activities_pkey");

            entity.Property(e => e.ActivityId)
                .HasColumnName("activityid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasColumnType("character varying");

            entity.Property(e => e.DurationMinutes)
                .IsRequired()
                .HasColumnName("durationminutes")
                .HasColumnType("integer");

            entity.Property(e => e.CaloriesBurned)
                .IsRequired()
                .HasColumnName("caloriesburned")
                .HasColumnType("integer");
        });

        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.ToTable("useractivities");

            entity.HasKey(e => e.UserActivityId).HasName("user_activities_pkey");

            entity.Property(e => e.UserActivityId)
                .HasColumnName("useractivityid")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("userid")
                .HasColumnType("uuid");

            entity.Property(e => e.ActivityId)
                .IsRequired()
                .HasColumnName("activityid")
                .HasColumnType("integer");

            entity.Property(e => e.ActivityDate)
                .IsRequired()
                .HasColumnName("activitydate")
                .HasColumnType("date");

            entity.Property(e => e.DurationMinutes)
                .IsRequired()
                .HasColumnName("durationminutes")
                .HasColumnType("integer");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_activities_user_id_fkey");

            entity.HasOne(d => d.Activity).WithMany(p => p.UserActivities)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("user_activities_activityid_fkey");

            // Unique constraint để tránh duplicate activity cho cùng 1 user và ngày
            entity.HasIndex(e => new { e.UserId, e.ActivityId, e.ActivityDate })
                .IsUnique()
                .HasDatabaseName("user_activities_user_activity_date_unique");
        });

        modelBuilder.Entity<PackageFeature>(entity =>
        {
            entity.ToTable("package_features");

            entity.HasKey(e => e.Id).HasName("package_features_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.FeatureName)
                .IsRequired()
                .HasColumnName("feature_name")
                .HasColumnType("character varying(500)");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true);

            entity.Property(e => e.DisplayOrder)
                .HasColumnName("display_order")
                .HasColumnType("integer")
                .HasDefaultValue(1);

            entity.HasIndex(e => new { e.IsActive, e.DisplayOrder })
                .HasDatabaseName("idx_package_features_active_order");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
