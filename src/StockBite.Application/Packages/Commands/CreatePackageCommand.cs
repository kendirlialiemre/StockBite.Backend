using FluentValidation;
using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Packages.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Packages.Commands;

public class CreatePackageCommand : IRequest<PackageDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationDays { get; set; }
    public List<int> ModuleTypes { get; set; } = [];
}

public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ModuleTypes).NotEmpty().WithMessage("En az bir modül seçmelisiniz.");
    }
}

public class CreatePackageCommandHandler(IApplicationDbContext db) : IRequestHandler<CreatePackageCommand, PackageDto>
{
    public async Task<PackageDto> Handle(CreatePackageCommand request, CancellationToken ct)
    {
        var package = new Package { Name = request.Name, Description = request.Description, Price = request.Price, DurationDays = request.DurationDays };
        foreach (var mt in request.ModuleTypes.Distinct())
            package.Modules.Add(new PackageModule { PackageId = package.Id, ModuleType = (ModuleType)mt });

        db.Packages.Add(package);
        await db.SaveChangesAsync(ct);

        return new PackageDto(package.Id, package.Name, package.Description, package.Price, package.DurationDays, package.IsActive,
            package.Modules.Select(m => new PackageModuleDto((int)m.ModuleType, m.ModuleType.ToString())).ToList());
    }
}
