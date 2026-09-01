using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.Interfaces;


namespace FairShare.Application.Services;

public class DebtSimplificationService : IDebtSimplificationService
{
    private const decimal Epsilon = 0.005m; // half a cent — treat as settled below this

    public List<(Guid FromUserId, Guid ToUserId, decimal Amount)> Simplify(Dictionary<Guid, decimal> netBalances)
    {
        var creditors = new List<(Guid UserId, decimal Amount)>();
        var debtors = new List<(Guid UserId, decimal Amount)>();

        foreach (var (userId, balance) in netBalances)
        {
            if (balance > Epsilon) creditors.Add((userId, balance));
            else if (balance < -Epsilon) debtors.Add((userId, -balance));
        }

        var transactions = new List<(Guid, Guid, decimal)>();

        creditors = creditors.OrderByDescending(c => c.Amount).ToList();
        debtors = debtors.OrderByDescending(d => d.Amount).ToList();

        int ci = 0, di = 0;
        while (ci < creditors.Count && di < debtors.Count)
        {
            var (creditorId, creditAmount) = creditors[ci];
            var (debtorId, debtAmount) = debtors[di];

            var settleAmount = Math.Min(creditAmount, debtAmount);
            transactions.Add((debtorId, creditorId, Math.Round(settleAmount, 2)));

            creditors[ci] = (creditorId, creditAmount - settleAmount);
            debtors[di] = (debtorId, debtAmount - settleAmount);

            if (creditors[ci].Amount <= Epsilon) ci++;
            if (debtors[di].Amount <= Epsilon) di++;
        }

        return transactions;
    }
}
