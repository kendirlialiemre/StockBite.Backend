using StockBite.Domain.Enums;

namespace StockBite.Application.Common.Exceptions;

public class ModuleNotSubscribedException(ModuleType module)
    : Exception($"Aboneliğiniz {module} modülünü içermiyor.");
