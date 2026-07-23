using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FairShare.Domain.Entities;

namespace FairShare.Application.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateAccessToken(AppUser user);
    (string token, DateTime expiresAt) GenerateRefreshToken();
}