using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Public;

public record PublicMenuCategoryDto(Guid Id, string Name, int DisplayOrder, List<PublicMenuItemDto> Items);
public record PublicMenuItemDto(Guid Id, string Name, string? Description, decimal Price, string? ImageUrl, bool IsAvailable);
public record PublicMenuDto(string TenantName, string Slug, int QrMenuTemplate, string? LogoUrl, string PrimaryColor, string BgColor, string TextColor, string FontFamily, List<PublicMenuCategoryDto> Categories);

public record GetPublicMenuQuery(string Slug, Guid? QrCodeId) : IRequest<PublicMenuDto?>;

public class GetPublicMenuQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPublicMenuQuery, PublicMenuDto?>
{
    public async Task<PublicMenuDto?> Handle(GetPublicMenuQuery request, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == request.Slug && t.IsActive, ct);

        if (tenant == null) return null;

        var categories = await db.MenuCategories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenant.Id && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.DisplayOrder,
                Items = db.MenuItems
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(i => i.CategoryId == c.Id && i.IsAvailable)
                    .OrderBy(i => i.Name)
                    .Select(i => new PublicMenuItemDto(i.Id, i.Name, i.Description, i.Price, i.ImageUrl, i.IsAvailable))
                    .ToList()
            })
            .ToListAsync(ct);

        var categoryDtos = categories
            .Select(c => new PublicMenuCategoryDto(c.Id, c.Name, c.DisplayOrder, c.Items))
            .ToList();

        // Per-QR design override
        int template = tenant.QrMenuTemplate;
        string? logoUrl = tenant.LogoUrl;
        string primary = tenant.PrimaryColor;
        string bg = tenant.BgColor;
        string text = tenant.TextColor;
        string font = tenant.FontFamily;

        if (request.QrCodeId.HasValue)
        {
            var qr = await db.MenuQrCodes
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(q => q.Id == request.QrCodeId.Value && q.TenantId == tenant.Id, ct);

            if (qr != null)
            {
                template = qr.QrMenuTemplate ?? template;
                logoUrl = qr.LogoUrl ?? logoUrl;
                primary = qr.PrimaryColor ?? primary;
                bg = qr.BgColor ?? bg;
                text = qr.TextColor ?? text;
                font = qr.FontFamily ?? font;
            }
        }

        return new PublicMenuDto(tenant.Name, tenant.Slug, template, logoUrl, primary, bg, text, font, categoryDtos);
    }
}
