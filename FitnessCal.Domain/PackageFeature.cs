using System;

namespace FitnessCal.Domain
{
    public class PackageFeature
    {
        public int Id { get; set; }
        public string FeatureName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;
    }
}


