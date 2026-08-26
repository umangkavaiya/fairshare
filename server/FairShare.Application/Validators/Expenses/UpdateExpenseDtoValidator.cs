using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FairShare.Application.DTOs.Expenses;

namespace FairShare.Application.Validators.Expenses;

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.SplitType).Must(t => new[] { "Equal", "Exact", "Percentage" }.Contains(t));
        RuleFor(x => x.Participants).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion is required to prevent overwriting concurrent edits.");
    }
}
