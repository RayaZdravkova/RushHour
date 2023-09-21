using Microsoft.AspNetCore.Mvc;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Accounts;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> CheckAccountAsync(AccountRequestDtoForLogin dto)
        {
            var checkAccount = await _authService.GetTokenAsync(dto);

            return Ok(checkAccount);
        }

    }
}
