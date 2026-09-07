using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.DTOs.Budgets;
using FluentValidation;

namespace FairShare.Application.Validators.Budgets;

public class CreateBudgetDtoValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
    }
}

public class UpdateBudgetDtoValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetDtoValidator()
    {
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
