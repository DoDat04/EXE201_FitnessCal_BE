using System;

namespace FitnessCal.BLL.DTO.UserWeightLogDTO.Response
{
    public class WeightLogResponseDTO
    {
        public DateOnly LogDate { get; set; }
        public decimal WeightKg { get; set; }
    }
}
