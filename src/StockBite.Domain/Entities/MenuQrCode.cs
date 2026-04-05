namespace StockBite.Domain.Entities;

public class MenuQrCode : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;

    // Per-QR design overrides (null = use tenant default)
    public int? QrMenuTemplate { get; set; }
    public string? PrimaryColor { get; set; }
    public string? BgColor { get; set; }
    public string? TextColor { get; set; }
    public string? FontFamily { get; set; }
    public string? LogoUrl { get; set; }
}
