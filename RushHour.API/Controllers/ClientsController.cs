using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RushHour.API.Constants;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Clients;
using RushHour.Domain.Pagination;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Client}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var client = await _clientService.GetByIdAsync(id);

            return Ok(client);
        }

        [HttpGet]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PagingInfo pagingInfo)
        {
            var clients = await _clientService.GetAllAsync(pagingInfo);

            return Ok(clients);
        }

        [HttpPost]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]
        public async Task<IActionResult> CreateAsync(ClientRequestDto dto)
        {
            var createdClient = await _clientService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdClient.Id }, createdClient);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Client}")]
        public async Task<IActionResult> UpdateAsync(int id, ClientRequestDtoForUpdate dto)
        {
            var updatedClient = await _clientService.UpdateAsync(id, dto);

            return Ok(updatedClient);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Client}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _clientService.DeleteAsync(id);

            return NoContent();
        }
    }
}
