using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FairShare.Application.DTOs.Settlements;

namespace FairShare.Application.Validators.Settlements;

public class CreateSettlementDtoValidator : AbstractValidator<CreateSettlementDto>
{
    public CreateSettlementDtoValidator()
    {
        RuleFor(x => x.PayerUserId).NotEmpty();
        RuleFor(x => x.PayeeUserId).NotEmpty();
        RuleFor(x => x).Must(x => x.PayerUserId != x.PayeeUserId)
            .WithMessage("A user cannot record a settlement with themselves.");
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.SettledAt).NotEmpty();
    }
}
