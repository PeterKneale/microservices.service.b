using FluentValidation;

namespace ServiceB.Api.Validators;

public class GetItemValidator : AbstractValidator<GetItemRequest>
{
    public GetItemValidator()
    {
        RuleFor(x => x.Message).NotEmpty();
    }
}