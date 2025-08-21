using FluentValidation;
using WaterJugChallenge.Models;

namespace WaterJugChallenge.Validators;

public class WaterJugRequestValidator : AbstractValidator<WaterJugRequest>
{
    public WaterJugRequestValidator()
    {
        RuleFor(x => x.XCapacity)
            .GreaterThan(0)
            .WithMessage("X capacity must be a positive integer");

        RuleFor(x => x.YCapacity)
            .GreaterThan(0)
            .WithMessage("Y capacity must be a positive integer");

        RuleFor(x => x.ZAmountWanted)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Target amount must be a non-negative integer");

        RuleFor(x => x)
            .Must(request => request.ZAmountWanted <= Math.Max(request.XCapacity, request.YCapacity))
            .WithMessage("Target amount cannot exceed the capacity of the larger jug")
            .When(x => x.XCapacity > 0 && x.YCapacity > 0 && x.ZAmountWanted >= 0);
    }
}