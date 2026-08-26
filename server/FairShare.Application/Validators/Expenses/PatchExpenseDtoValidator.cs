using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FairShare.Application.DTOs.Expenses;

namespace FairShare.Application.Validators.Expenses;

public class PatchExpenseDtoValidator : AbstractValidator<PatchExpenseDto>
{
    public PatchExpenseDtoValidator()
    {
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
