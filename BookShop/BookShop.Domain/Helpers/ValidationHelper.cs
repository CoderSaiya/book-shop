using System.ComponentModel.DataAnnotations;

namespace BookShop.Domain.Helpers;

public static class ValidationHelper
{
    public static void Validate(params (bool isInvalid, string errorMessage)[] checks)
    {
        var errors = checks
            .Where(c => c.isInvalid)
            .Select(c => c.errorMessage)
            .ToList();

        if (errors.Any())
            throw new ValidationException(string.Join(" | ", errors));
    }
}