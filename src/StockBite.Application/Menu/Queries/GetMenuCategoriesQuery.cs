using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;

namespace StockBite.Application.Menu.Queries;

public record GetMenuCategoriesQuery : IRequest<List<MenuCategoryDto>>;

public class GetMenuCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMenuCategoriesQuery, List<MenuCategoryDto>>
{
    public async Task<List<MenuCategoryDto>> Handle(GetMenuCategoriesQuery request, CancellationToken ct)
    {
        return await db.MenuCategories
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new MenuCategoryDto(c.Id, c.Name, c.DisplayOrder, c.IsActive))
            .ToListAsync(ct);
    }
}
