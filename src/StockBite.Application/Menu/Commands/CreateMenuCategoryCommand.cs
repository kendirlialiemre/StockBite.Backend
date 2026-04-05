using FluentValidation;
using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record CreateMenuCategoryCommand(string Name, int DisplayOrder = 0) : IRequest<MenuCategoryDto>;

public class CreateMenuCategoryValidator : AbstractValidator<CreateMenuCategoryCommand>
{
    public CreateMenuCategoryValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); }
}

public class CreateMenuCategoryCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateMenuCategoryCommand, MenuCategoryDto>
{
    public async Task<MenuCategoryDto> Handle(CreateMenuCategoryCommand request, CancellationToken ct)
    {
        var category = new MenuCategory
        {
            TenantId = currentUser.TenantId!.Value,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder
        };
        db.MenuCategories.Add(category);
        await db.SaveChangesAsync(ct);
        return new MenuCategoryDto(category.Id, category.Name, category.DisplayOrder, category.IsActive);
    }
}
