namespace StockBite.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int QrMenuTemplate { get; set; } = 1; // 1=Minimal, 2=Modern, 3=Classic
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#0f172a";
    public string BgColor { get; set; } = "#f9fafb";
    public string TextColor { get; set; } = "#1e293b";
    public string FontFamily { get; set; } = "sans";
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<TenantModule> Modules { get; set; } = new List<TenantModule>();
}
