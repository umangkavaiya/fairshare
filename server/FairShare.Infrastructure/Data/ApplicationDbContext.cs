using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FairShare.Domain.Entities;

namespace FairShare.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetPeriod> BudgetPeriods => Set<BudgetPeriod>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<FinancialAccountCategory> FinancialAccountCategories => Set<FinancialAccountCategory>();
    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<FinancialAccountValue> FinancialAccountValues => Set<FinancialAccountValue>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(x => x.Token).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Group>(e =>
        {
            e.HasOne(g => g.CreatedBy).WithMany()
             .HasForeignKey(g => g.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GroupMember>(e =>
        {
            e.HasIndex(gm => gm.UserId);
            e.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();
            e.HasOne(gm => gm.Group).WithMany(g => g.Members)
             .HasForeignKey(gm => gm.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gm => gm.User).WithMany()
             .HasForeignKey(gm => gm.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Expense>(e =>
        {
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.GroupId);
            e.HasIndex(x => x.PaidByUserId);
            e.HasIndex(x => x.ExpenseDate);
            e.HasOne(x => x.Group).WithMany(g => g.Expenses)
             .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PaidBy).WithMany()
             .HasForeignKey(x => x.PaidByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany()
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.SubscriptionId, x.ExpenseDate }).IsUnique();
            e.HasOne(x => x.Subscription).WithMany()
             .HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ExpenseSplit>(e =>
        {
            e.Property(x => x.AmountOwed).HasColumnType("decimal(18,2)");
            e.Property(x => x.Percentage).HasColumnType("decimal(5,2)");
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.ExpenseId, x.UserId }).IsUnique();
            e.HasOne(x => x.Expense).WithMany(x => x.Splits)
             .HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<IdempotencyKey>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.ResponseBody).HasColumnType("nvarchar(max)");
            e.HasIndex(x => x.ExpiresAt);
        });

        // seed a few default categories
        builder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "Food & Drink", Icon = "utensils" },
            new ExpenseCategory { Id = 2, Name = "Transport", Icon = "car" },
            new ExpenseCategory { Id = 3, Name = "Lodging", Icon = "bed" },
            new ExpenseCategory { Id = 4, Name = "Utilities", Icon = "bolt" },
            new ExpenseCategory { Id = 5, Name = "Entertainment", Icon = "film" },
            new ExpenseCategory { Id = 6, Name = "Other", Icon = "tag" }
        );

        builder.Entity<Settlement>(e =>
        {
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
            e.HasIndex(x => x.GroupId);
            e.HasIndex(x => x.PayerUserId);
            e.HasIndex(x => x.PayeeUserId);

            e.HasOne(x => x.Group).WithMany()
             .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);

            // Two FKs to the same AppUser table — each needs an explicit, distinct
            // navigation and DeleteBehavior.Restrict, or EF can't disambiguate them.
            e.HasOne(x => x.Payer).WithMany()
             .HasForeignKey(x => x.PayerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Payee).WithMany()
             .HasForeignKey(x => x.PayeeUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Budget>(e =>
        {
            e.Property(x => x.MonthlyLimit).HasColumnType("decimal(18,2)");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.UserId);
            // one active budget per category per user — filtered so a deactivated
            // budget doesn't block recreating one for the same category
            e.HasIndex(x => new { x.UserId, x.CategoryId })
             .IsUnique()
             .HasFilter("[IsActive] = 1");
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany()
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BudgetPeriod>(e =>
        {
            e.Property(x => x.CarriedInAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.EffectiveLimit).HasColumnType("decimal(18,2)");
            e.Property(x => x.SpentAmount).HasColumnType("decimal(18,2)");
            e.HasIndex(x => new { x.BudgetId, x.PeriodStart }).IsUnique();
            e.HasOne(x => x.Budget).WithMany(b => b.Periods)
             .HasForeignKey(x => x.BudgetId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Subscription>(e =>
        {
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.NextBillingDate);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany()
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        // seed default net-worth categories
        builder.Entity<FinancialAccountCategory>().HasData(
            new FinancialAccountCategory { Id = 1, Name = "Cash & Bank", Icon = "wallet", AppliesToType = FinancialAccountType.Asset },
            new FinancialAccountCategory { Id = 2, Name = "Investment", Icon = "trending-up", AppliesToType = FinancialAccountType.Asset },
            new FinancialAccountCategory { Id = 3, Name = "Property", Icon = "home", AppliesToType = FinancialAccountType.Asset },
            new FinancialAccountCategory { Id = 4, Name = "Vehicle", Icon = "car", AppliesToType = FinancialAccountType.Asset },
            new FinancialAccountCategory { Id = 5, Name = "Loan", Icon = "file-text", AppliesToType = FinancialAccountType.Liability },
            new FinancialAccountCategory { Id = 6, Name = "Credit Card", Icon = "credit-card", AppliesToType = FinancialAccountType.Liability },
            new FinancialAccountCategory { Id = 7, Name = "Other", Icon = "tag", AppliesToType = null }
        );

        builder.Entity<FinancialAccount>(e =>
        {
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany()
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FinancialAccountValue>(e =>
        {
            e.Property(x => x.Value).HasColumnType("decimal(18,2)");
            e.HasIndex(x => new { x.FinancialAccountId, x.AsOfDate });
            e.HasOne(x => x.FinancialAccount).WithMany(a => a.Values)
             .HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}