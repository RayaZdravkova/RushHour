using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RushHour.API.Constants;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Employees;
using RushHour.Domain.Pagination;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin},{AuthorizationRoles.Employee}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            return Ok(employee);
        }

        [HttpGet]
        [Authorize(Roles = $"{AuthorizationRoles.Admin}")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PagingInfo pagingInfo)
        {
            var employees = await _employeeService.GetAllAsync(pagingInfo);

            return Ok(employees);
        }

        [HttpPost]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> CreateAsync(EmployeeRequestDto dto)
        {
            var createdEmployee = await _employeeService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin},{AuthorizationRoles.Employee}")]
        public async Task<IActionResult> UpdateAsync(int id, EmployeeRequestDtoForUpdate dto)
        {
            var updatedEmployee = await _employeeService.UpdateAsync(id, dto);
           
            return Ok(updatedEmployee);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{AuthorizationRoles.Admin},{AuthorizationRoles.ProviderAdmin}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
           await _employeeService.DeleteAsync(id);

           return NoContent();
        }
    }
}
