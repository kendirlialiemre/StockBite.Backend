using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record DeleteMenuCategoryCommand(Guid Id) : IRequest;

public class DeleteMenuCategoryCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteMenuCategoryCommand>
{
    public async Task Handle(DeleteMenuCategoryCommand request, CancellationToken ct)
    {
        var category = await db.MenuCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuCategory), request.Id);

        db.MenuCategories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
