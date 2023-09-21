using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RushHour.API.Constants;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Providers;
using RushHour.Domain.Pagination;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;
        public ProvidersController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var provider = await _providerService.GetByIdAsync(id);

            return Ok(provider);
        }

        [HttpGet]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Client}")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PagingInfo pagingInfo)
        {
            var providers = await _providerService.GetAllAsync(pagingInfo);

            return Ok(providers);
        }

        [HttpPost]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]

        public async Task<IActionResult> CreateAsync(ProviderRequestDto dto)
        {
            var createdProvider = await _providerService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdProvider.Id }, createdProvider);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> UpdateAsync(int id, ProviderRequestDto dto)
        {
            var updatedProvider = await _providerService.UpdateAsync(id, dto);
           
            return Ok(updatedProvider);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _providerService.DeleteAsync(id);
            
            return NoContent();
        }
    }
}
