namespace StockBite.Application.Common.Exceptions;

public class NotFoundException(string name, object key)
    : Exception($"\"{name}\" ({key}) bulunamadı.");
