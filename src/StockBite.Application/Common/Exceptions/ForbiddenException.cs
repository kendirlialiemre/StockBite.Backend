namespace StockBite.Application.Common.Exceptions;

public class ForbiddenException(string message = "Erişim reddedildi.") : Exception(message);
