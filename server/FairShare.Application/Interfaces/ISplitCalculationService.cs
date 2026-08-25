using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.Interfaces;

public interface ISplitCalculationService
{
    List<(Guid UserId, decimal Amount)> CalculateEqualSplit(decimal total, List<Guid> userIds);
    List<(Guid UserId, decimal Amount)> CalculateExactSplit(decimal total, Dictionary<Guid, decimal> amounts);
    List<(Guid UserId, decimal Amount, decimal Percentage)> CalculatePercentageSplit(decimal total, Dictionary<Guid, decimal> percentages);
}