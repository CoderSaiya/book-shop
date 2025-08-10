namespace BookShop.Domain.Common;

public abstract class AppException(string message) : Exception(message);

public sealed class EmailAlreadyUsedException(string email)
    : AppException($"Email '{email}' đã được sử dụng.");

public sealed class NotFoundException(string name, object key)
    : AppException($"{name} với khóa '{key}' không tồn tại.");

public sealed class DomainValidationException(string message)
    : AppException(message);