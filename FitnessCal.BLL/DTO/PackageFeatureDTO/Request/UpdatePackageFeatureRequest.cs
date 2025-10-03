namespace FitnessCal.BLL.DTO.PackageFeatureDTO.Request;

public class UpdatePackageFeatureRequest
{
    public string FeatureName { get; set; } = null!;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}


