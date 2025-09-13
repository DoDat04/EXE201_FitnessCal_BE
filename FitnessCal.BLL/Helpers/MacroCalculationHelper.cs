using System;

namespace FitnessCal.BLL.Helpers
{
    public static class MacroCalculationHelper
    {
        /// <summary>
        /// Tính protein mục tiêu từ daily calories
        /// </summary>
        /// <param name="dailyCalories">Tổng calories hàng ngày</param>
        /// <returns>Protein mục tiêu (grams)</returns>
        public static double CalculateTargetProtein(double dailyCalories)
        {
            // 15-25% của daily calories, 1g protein = 4 cal
            return (dailyCalories * 0.20) / 4;
        }

        /// <summary>
        /// Tính carbs mục tiêu từ daily calories
        /// </summary>
        /// <param name="dailyCalories">Tổng calories hàng ngày</param>
        /// <returns>Carbs mục tiêu (grams)</returns>
        public static double CalculateTargetCarbs(double dailyCalories)
        {
            // 45-65% của daily calories, 1g carbs = 4 cal
            return (dailyCalories * 0.55) / 4;
        }

        /// <summary>
        /// Tính fat mục tiêu từ daily calories
        /// </summary>
        /// <param name="dailyCalories">Tổng calories hàng ngày</param>
        /// <returns>Fat mục tiêu (grams)</returns>
        public static double CalculateTargetFat(double dailyCalories)
        {
            // 20-35% của daily calories, 1g fat = 9 cal
            return (dailyCalories * 0.275) / 9;
        }

        /// <summary>
        /// Tính tất cả macro targets từ daily calories
        /// </summary>
        /// <param name="dailyCalories">Tổng calories hàng ngày</param>
        /// <returns>MacroTargetDTO với protein, carbs, fat</returns>
        public static MacroTargetDTO CalculateAllMacroTargets(double dailyCalories)
        {
            return new MacroTargetDTO
            {
                Protein = Math.Round(CalculateTargetProtein(dailyCalories), 1),
                Carbs = Math.Round(CalculateTargetCarbs(dailyCalories), 1),
                Fat = Math.Round(CalculateTargetFat(dailyCalories), 1)
            };
        }
    }

    /// <summary>
    /// DTO chứa thông tin macro targets
    /// </summary>
    public class MacroTargetDTO
    {
        public double Protein { get; set; }  // grams
        public double Carbs { get; set; }    // grams  
        public double Fat { get; set; }      // grams
    }
}
