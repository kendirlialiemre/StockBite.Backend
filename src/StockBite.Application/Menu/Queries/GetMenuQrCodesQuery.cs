using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Menu.Queries;

public record MenuQrCodeDto(Guid Id, string Label, string FilePath, string PublicUrl, DateTime CreatedAt);

public record GetMenuQrCodesQuery : IRequest<List<MenuQrCodeDto>>;

public class GetMenuQrCodesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMenuQrCodesQuery, List<MenuQrCodeDto>>
{
    public async Task<List<MenuQrCodeDto>> Handle(GetMenuQrCodesQuery request, CancellationToken ct)
    {
        return await db.MenuQrCodes
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new MenuQrCodeDto(q.Id, q.Label, q.FilePath, q.PublicUrl, q.CreatedAt))
            .ToListAsync(ct);
    }
}
