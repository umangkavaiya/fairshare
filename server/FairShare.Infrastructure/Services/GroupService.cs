using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Groups;
using FairShare.Application.DTOs.Users;
using FairShare.Application.Interfaces;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;

namespace FairShare.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _db;

    public GroupService(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<GroupResponseDto>> GetGroupsForUserAsync(Guid userId, int page, int pageSize)
    {
        var query = _db.GroupMembers
            .Where(gm => gm.UserId == userId && gm.IsActive)
            .Select(gm => gm.Group)
            .Where(g => g.IsActive);

        var totalCount = await query.CountAsync();

        var groups = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GroupResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                DefaultCurrency = g.DefaultCurrency,
                CreatedByUserId = g.CreatedByUserId,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count(m => m.IsActive),
                CurrentUserRole = g.Members.First(m => m.UserId == userId && m.IsActive).Role.ToString()
            })
            .ToListAsync();

        return new PagedResult<GroupResponseDto> { Items = groups, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<GroupResponseDto> GetGroupByIdAsync(Guid groupId, Guid userId)
    {
        var membership = await GetActiveMembershipAsync(groupId, userId);

        var group = await _db.Groups
            .Where(g => g.Id == groupId && g.IsActive)
            .Select(g => new GroupResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                DefaultCurrency = g.DefaultCurrency,
                CreatedByUserId = g.CreatedByUserId,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count(m => m.IsActive),
                CurrentUserRole = membership.Role.ToString()
            })
            .FirstOrDefaultAsync();

        return group ?? throw new KeyNotFoundException("Group not found.");
    }

    public async Task<GroupResponseDto> CreateGroupAsync(Guid userId, CreateGroupDto dto)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var group = new Group
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultCurrency = dto.DefaultCurrency,
            CreatedByUserId = userId
        };
        _db.Groups.Add(group);

        _db.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = userId, Role = GroupRole.Admin });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetGroupByIdAsync(group.Id, userId);
    }

    public async Task<GroupResponseDto> UpdateGroupAsync(Guid groupId, Guid userId, UpdateGroupDto dto)
    {
        await RequireAdminAsync(groupId, userId);
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive)
            ?? throw new KeyNotFoundException("Group not found.");

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.DefaultCurrency = dto.DefaultCurrency;

        await _db.SaveChangesAsync();
        return await GetGroupByIdAsync(groupId, userId);
    }

    public async Task<GroupResponseDto> PatchGroupAsync(Guid groupId, Guid userId, PatchGroupDto dto)
    {
        await RequireAdminAsync(groupId, userId);
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive)
            ?? throw new KeyNotFoundException("Group not found.");

        if (dto.Name is not null) group.Name = dto.Name;
        if (dto.Description is not null) group.Description = dto.Description;
        if (dto.DefaultCurrency is not null) group.DefaultCurrency = dto.DefaultCurrency;

        await _db.SaveChangesAsync();
        return await GetGroupByIdAsync(groupId, userId);
    }

    public async Task ArchiveGroupAsync(Guid groupId, Guid userId)
    {
        await RequireAdminAsync(groupId, userId);
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive)
            ?? throw new KeyNotFoundException("Group not found.");

        group.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<List<GroupMemberDto>> GetMembersAsync(Guid groupId, Guid userId)
    {
        await GetActiveMembershipAsync(groupId, userId);

        return await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .OrderBy(gm => gm.JoinedAt)
            .Select(gm => new GroupMemberDto
            {
                UserId = gm.UserId,
                DisplayName = gm.User.DisplayName,
                Email = gm.User.Email ?? string.Empty,
                Role = gm.Role.ToString(),
                JoinedAt = gm.JoinedAt
            })
            .ToListAsync();
    }

    public async Task<GroupMemberDto> InviteMemberAsync(Guid groupId, Guid userId, InviteMemberDto dto)
    {
        await RequireAdminAsync(groupId, userId);

        var invitee = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new KeyNotFoundException("No FairShare account found with that email. Ask them to register first.");

        var existingMembership = await _db.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == invitee.Id);

        if (existingMembership is not null)
        {
            if (existingMembership.IsActive)
                throw new ArgumentException("This user is already a member of the group.");

            existingMembership.IsActive = true;
            existingMembership.Role = GroupRole.Member;
            existingMembership.JoinedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new GroupMemberDto
            {
                UserId = invitee.Id,
                DisplayName = invitee.DisplayName,
                Email = invitee.Email ?? string.Empty,
                Role = existingMembership.Role.ToString(),
                JoinedAt = existingMembership.JoinedAt
            };
        }

        var membership = new GroupMember { GroupId = groupId, UserId = invitee.Id, Role = GroupRole.Member };
        _db.GroupMembers.Add(membership);
        await _db.SaveChangesAsync();

        return new GroupMemberDto
        {
            UserId = invitee.Id,
            DisplayName = invitee.DisplayName,
            Email = invitee.Email ?? string.Empty,
            Role = membership.Role.ToString(),
            JoinedAt = membership.JoinedAt
        };
    }

    public async Task RemoveMemberAsync(Guid groupId, Guid userId, Guid memberUserId)
    {
        await RequireAdminAsync(groupId, userId);
        await DeactivateMembershipAsync(groupId, memberUserId);
    }

    public async Task LeaveGroupAsync(Guid groupId, Guid userId)
    {
        await GetActiveMembershipAsync(groupId, userId);
        await DeactivateMembershipAsync(groupId, userId);
    }

    public async Task<List<UserSearchResultDto>> SearchUsersAsync(Guid groupId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2) return new List<UserSearchResultDto>();

        var existingMemberIds = await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .Select(gm => gm.UserId)
            .ToListAsync();

        return await _db.Users
            .Where(u => !existingMemberIds.Contains(u.Id) && (u.Email!.Contains(query) || u.DisplayName.Contains(query)))
            .Take(10)
            .Select(u => new UserSearchResultDto { Id = u.Id, DisplayName = u.DisplayName, Email = u.Email ?? string.Empty })
            .ToListAsync();
    }

    private async Task<GroupMember> GetActiveMembershipAsync(Guid groupId, Guid userId)
    {
        var membership = await _db.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);
        return membership ?? throw new UnauthorizedAccessException("You are not a member of this group.");
    }

    private async Task RequireAdminAsync(Guid groupId, Guid userId)
    {
        var membership = await GetActiveMembershipAsync(groupId, userId);
        if (membership.Role != GroupRole.Admin)
            throw new UnauthorizedAccessException("Only group admins can perform this action.");
    }

    private async Task DeactivateMembershipAsync(Guid groupId, Guid targetUserId)
    {
        // Non-zero-balance restriction (backlog stories 2.4/2.5) gets enforced here
        // once the debt-simplification engine exists in Phase 5.
        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId && gm.IsActive)
            ?? throw new KeyNotFoundException("Membership not found.");

        membership.IsActive = false;
        await _db.SaveChangesAsync();
    }
}

