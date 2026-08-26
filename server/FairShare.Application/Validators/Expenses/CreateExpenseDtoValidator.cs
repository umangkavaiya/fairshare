using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FairShare.Application.DTOs.Expenses;

namespace FairShare.Application.Validators.Expenses;

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.SplitType).Must(t => new[] { "Equal", "Exact", "Percentage" }.Contains(t))
            .WithMessage("SplitType must be Equal, Exact, or Percentage.");
        RuleFor(x => x.Participants).NotEmpty().WithMessage("At least one participant is required.");
        RuleFor(x => x.PaidByUserId).NotEmpty();
    }
}
