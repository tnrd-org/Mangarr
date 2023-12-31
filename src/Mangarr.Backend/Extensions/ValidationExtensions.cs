using FluentValidation.Results;

namespace Mangarr.Backend.Extensions;

public class ValidationResponse
{
    public string Property { get; init; } = default!;
    public string Message { get; init; } = default!;
}

public static class ValidationExtensions
{
    public static List<ValidationResponse> ToResponse(this IEnumerable<ValidationFailure> errors)
    {
        List<ValidationResponse> list = new();

        foreach (ValidationFailure error in errors)
        {
            list.Add(new ValidationResponse { Property = error.PropertyName, Message = error.ErrorMessage });
        }

        return list;
    }
}
