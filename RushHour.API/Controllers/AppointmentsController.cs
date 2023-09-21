using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RushHour.API.Constants;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.Pagination;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Employee},{AuthorizationRoles.Client},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);

            return Ok(appointment);
        }

        [HttpGet]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PagingInfo pagingInfo)
        {
            var appointments = await _appointmentService.GetAllAsync(pagingInfo);

            return Ok(appointments);
        }

        [HttpPost]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Employee},{AuthorizationRoles.Client},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> CreateAsync(AppointmentCreateRequestDto dto)
        {
            var createdAppointments = await _appointmentService.CreateAsync(dto);

            return Ok(createdAppointments);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Employee},{AuthorizationRoles.Client},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> UpdateAsync(int id, AppointmentRequestDto dto)
        {
            var updatedAppointment = await _appointmentService.UpdateAsync(id, dto);

            return Ok(updatedAppointment);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.Employee},{AuthorizationRoles.Client},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _appointmentService.DeleteAsync(id);

            return NoContent();
        }
    }
}
