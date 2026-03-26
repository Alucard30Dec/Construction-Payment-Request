namespace ConstructionPayment.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string UploadPath { get; set; } = "uploads";
}
