using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Groups;
using FairShare.Application.DTOs.Users;

namespace FairShare.Application.Interfaces;

public interface IGroupService
{
    Task<PagedResult<GroupResponseDto>> GetGroupsForUserAsync(Guid userId, int page, int pageSize);
    Task<GroupResponseDto> GetGroupByIdAsync(Guid groupId, Guid userId);
    Task<GroupResponseDto> CreateGroupAsync(Guid userId, CreateGroupDto dto);
    Task<GroupResponseDto> UpdateGroupAsync(Guid groupId, Guid userId, UpdateGroupDto dto);
    Task<GroupResponseDto> PatchGroupAsync(Guid groupId, Guid userId, PatchGroupDto dto);
    Task ArchiveGroupAsync(Guid groupId, Guid userId);
    Task<List<GroupMemberDto>> GetMembersAsync(Guid groupId, Guid userId);
    Task<GroupMemberDto> InviteMemberAsync(Guid groupId, Guid userId, InviteMemberDto dto);
    Task RemoveMemberAsync(Guid groupId, Guid userId, Guid memberUserId);
    Task LeaveGroupAsync(Guid groupId, Guid userId);
    Task<List<UserSearchResultDto>> SearchUsersAsync(Guid groupId, string query);
}