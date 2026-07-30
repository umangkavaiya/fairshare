using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FairShare.Application.DTOs.Groups;

namespace FairShare.Application.Validators.Groups;

public class InviteMemberDtoValidator : AbstractValidator<InviteMemberDto>
{
    public InviteMemberDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}