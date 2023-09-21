using Microsoft.AspNetCore.Mvc;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Accounts;

namespace RushHour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPatch("password")]
        public async Task<IActionResult> ChangePasswordAsync(AccountRequestDtoForPasswordUpdate dto)
        {
            await _accountService.ChangePassword(dto);

            return Ok();
        }
    }
}
