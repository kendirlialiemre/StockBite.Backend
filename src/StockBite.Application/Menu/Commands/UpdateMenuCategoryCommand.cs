using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record UpdateMenuCategoryCommand(Guid Id, string? Name, int? DisplayOrder, bool? IsActive)
    : IRequest<MenuCategoryDto>;

public class UpdateMenuCategoryCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMenuCategoryCommand, MenuCategoryDto>
{
    public async Task<MenuCategoryDto> Handle(UpdateMenuCategoryCommand request, CancellationToken ct)
    {
        var category = await db.MenuCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuCategory), request.Id);

        if (request.Name != null) category.Name = request.Name;
        if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        await db.SaveChangesAsync(ct);
        return new MenuCategoryDto(category.Id, category.Name, category.DisplayOrder, category.IsActive);
    }
}
