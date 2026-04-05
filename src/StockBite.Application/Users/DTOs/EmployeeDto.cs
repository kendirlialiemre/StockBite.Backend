namespace StockBite.Application.Users.DTOs;

public record EmployeeDto(Guid Id, string Email, string FirstName, string LastName, string Role, bool IsActive, IReadOnlyList<string> Permissions);
