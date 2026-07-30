using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FairShare.Api.Extensions;
using FairShare.Application.DTOs.Groups;
using FairShare.Application.Interfaces;

namespace FairShare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService) => _groupService = groupService;

    [HttpGet]
    public async Task<IActionResult> GetGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _groupService.GetGroupsForUserAsync(User.GetUserId(), page, pageSize));

    [HttpGet("{groupId:guid}")]
    public async Task<IActionResult> GetGroup(Guid groupId)
        => Ok(await _groupService.GetGroupByIdAsync(groupId, User.GetUserId()));

    [HttpPost]
    public async Task<IActionResult> CreateGroup(CreateGroupDto dto)
    {
        var group = await _groupService.CreateGroupAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetGroup), new { groupId = group.Id }, group);
    }

    [HttpPut("{groupId:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid groupId, UpdateGroupDto dto)
        => Ok(await _groupService.UpdateGroupAsync(groupId, User.GetUserId(), dto));

    [HttpPatch("{groupId:guid}")]
    public async Task<IActionResult> PatchGroup(Guid groupId, PatchGroupDto dto)
        => Ok(await _groupService.PatchGroupAsync(groupId, User.GetUserId(), dto));

    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> ArchiveGroup(Guid groupId)
    {
        await _groupService.ArchiveGroupAsync(groupId, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{groupId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid groupId)
        => Ok(await _groupService.GetMembersAsync(groupId, User.GetUserId()));

    [HttpPost("{groupId:guid}/members")]
    public async Task<IActionResult> InviteMember(Guid groupId, InviteMemberDto dto)
        => Ok(await _groupService.InviteMemberAsync(groupId, User.GetUserId(), dto));

    [HttpDelete("{groupId:guid}/members/{memberUserId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid memberUserId)
    {
        await _groupService.RemoveMemberAsync(groupId, User.GetUserId(), memberUserId);
        return NoContent();
    }

    [HttpPost("{groupId:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid groupId)
    {
        await _groupService.LeaveGroupAsync(groupId, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{groupId:guid}/members/search")]
    public async Task<IActionResult> SearchUsers(Guid groupId, [FromQuery] string query)
        => Ok(await _groupService.SearchUsersAsync(groupId, query));
}