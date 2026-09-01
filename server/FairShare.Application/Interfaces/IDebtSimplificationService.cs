using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.Interfaces;

public interface IDebtSimplificationService
{
    List<(Guid FromUserId, Guid ToUserId, decimal Amount)> Simplify(Dictionary<Guid, decimal> netBalances);
}
