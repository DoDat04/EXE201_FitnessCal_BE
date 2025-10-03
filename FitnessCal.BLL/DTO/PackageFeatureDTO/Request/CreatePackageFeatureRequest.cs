namespace FitnessCal.BLL.DTO.PackageFeatureDTO.Request;

public class CreatePackageFeatureRequest
{
    public string FeatureName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
}


