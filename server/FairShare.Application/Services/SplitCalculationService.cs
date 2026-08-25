using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.Interfaces;

namespace FairShare.Application.Services;

// Pure calculation logic — no EF Core, no DbContext. This is the class most worth
// unit-testing directly, since every currency-rounding edge case lives here.
public class SplitCalculationService : ISplitCalculationService
{
    public List<(Guid UserId, decimal Amount)> CalculateEqualSplit(decimal total, List<Guid> userIds)
    {
        if (userIds.Count == 0)
            throw new ArgumentException("At least one participant is required for an equal split.");

        int n = userIds.Count;
        long totalCents = ToCents(total);
        long baseShare = totalCents / n;
        long remainder = totalCents % n;

        // Largest-remainder method: the first `remainder` participants (by list order)
        // get one extra cent each, so the split always reconciles exactly to the total.
        var result = new List<(Guid, decimal)>();
        for (int i = 0; i < n; i++)
        {
            long shareCents = baseShare + (i < remainder ? 1 : 0);
            result.Add((userIds[i], FromCents(shareCents)));
        }

        ValidateReconciliation(total, result.Select(r => r.Item2));
        return result;
    }

    public List<(Guid UserId, decimal Amount)> CalculateExactSplit(decimal total, Dictionary<Guid, decimal> amounts)
    {
        if (amounts.Count == 0)
            throw new ArgumentException("At least one participant is required for an exact split.");

        var sum = amounts.Values.Sum();
        if (sum != total)
            throw new ArgumentException($"Exact split amounts ({sum:F2}) must sum to the expense total ({total:F2}).");

        return amounts.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public List<(Guid UserId, decimal Amount, decimal Percentage)> CalculatePercentageSplit(decimal total, Dictionary<Guid, decimal> percentages)
    {
        if (percentages.Count == 0)
            throw new ArgumentException("At least one participant is required for a percentage split.");

        var percentSum = percentages.Values.Sum();
        if (percentSum != 100m)
            throw new ArgumentException($"Percentages must sum to 100 (received {percentSum}).");

        long totalCents = ToCents(total);

        var raw = percentages.Select(kv =>
        {
            decimal exactCents = totalCents * (kv.Value / 100m);
            long flooredCents = (long)Math.Floor(exactCents);
            decimal fraction = exactCents - flooredCents;
            return new { UserId = kv.Key, kv.Value, FlooredCents = flooredCents, Fraction = fraction };
        }).ToList();

        long leftover = totalCents - raw.Sum(r => r.FlooredCents);

        // True largest-remainder method: whoever's fractional cent-share was closest
        // to rounding up gets the leftover pennies, not just "the first N in the list."
        var finalCents = raw.ToDictionary(r => r.UserId, r => r.FlooredCents);
        foreach (var r in raw.OrderByDescending(r => r.Fraction).Take((int)leftover))
            finalCents[r.UserId] += 1;

        var result = raw.Select(r => (r.UserId, FromCents(finalCents[r.UserId]), r.Value)).ToList();

        ValidateReconciliation(total, result.Select(r => r.Item2));
        return result;
    }

    private static long ToCents(decimal amount) => (long)Math.Round(amount * 100, 0, MidpointRounding.AwayFromZero);
    private static decimal FromCents(long cents) => cents / 100m;

    private static void ValidateReconciliation(decimal total, IEnumerable<decimal> amounts)
    {
        if (amounts.Sum() != total)
            throw new InvalidOperationException("Split calculation failed to reconcile with the expense total — this indicates a bug in the calculation engine, not bad input.");
    }
}
