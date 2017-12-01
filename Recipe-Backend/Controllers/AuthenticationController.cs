using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RecipeBackend.Models;
using RecipeBackend.Repositories;
using RecipeBackend.JwtAuthentication;
using Microsoft.Extensions.Options;

namespace RecipeBackend.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IOptions<Settings> _options;

        public AuthenticationController(IAccountRepository accountRepository, IOptions<Settings> options)
        {
            _accountRepository = accountRepository;
            _options = options;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]AccountInformation info)
        {
            bool success = await _accountRepository.TryValidatingAccount(info);

            if (!success)
                return Unauthorized();

            var token = new JwtTokenBuilder()
                                .AddSecurityKey(JwtSecurityKey.Create(_options.Value.JwtSecret))
                                .AddSubject(info.Username)
                                .AddIssuer(_options.Value.ApplicationName)
                                .AddAudience(_options.Value.ApplicationName)
                                .AddClaim("MembershipId", info.Username)
                                .AddExpiry(60)
                                .Build();

            return Ok(token.Value);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/signup")]
        public Task<bool> CreateAccount([FromBody]AccountInformation info)
        {
            return _accountRepository.TryAddAccountAsync(info);
        }

        [HttpGet]
        [Route("api/authenticate")]
        public bool AuthenticateJwt()
        {
            return true;
        }
    }
}
