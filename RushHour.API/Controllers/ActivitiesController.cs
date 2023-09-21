using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RushHour.API.Constants;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Activities;
using RushHour.Domain.Pagination;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;
        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var activity = await _activityService.GetByIdAsync(id);

            return Ok(activity);
        }

        [HttpGet]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Client}")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PagingInfo pagingInfo)
        {
            var activities = await _activityService.GetAllAsync(pagingInfo);

            return Ok(activities);
        }

        [HttpPost]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> CreateAsync(ActivityRequestDto dto)
        {
            var createdActivity = await _activityService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdActivity.Id }, createdActivity);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> UpdateAsync(int id, ActivityRequestDto dto)
        {
            var updatedActivity = await _activityService.UpdateAsync(id, dto);

            return Ok(updatedActivity);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _activityService.DeleteAsync(id);

            return NoContent();
        }
    }
}
