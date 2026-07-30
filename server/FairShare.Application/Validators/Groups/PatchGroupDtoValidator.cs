using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FairShare.Application.DTOs.Groups;

namespace FairShare.Application.Validators.Groups;

public class PatchGroupDtoValidator : AbstractValidator<PatchGroupDto>
{
    public PatchGroupDtoValidator()
    {
        RuleFor(x => x.Name).MaximumLength(150).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x.DefaultCurrency).Length(3).When(x => x.DefaultCurrency is not null);
    }
}