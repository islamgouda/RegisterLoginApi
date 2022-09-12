using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RegisterLoginApi.DTO;
using RegisterLoginApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RegisterLoginApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private IConfiguration config;

        public AccountController(UserManager<ApplicationUser> userManager,IConfiguration _config)
        {
            this.userManager = userManager;
            this.config = _config;
        }
        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            return Ok("hello islam");
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.Email = registerDTO.Email;
                applicationUser.UserName = registerDTO.Name;
             IdentityResult result=  await userManager.CreateAsync(applicationUser, registerDTO.Password);
                if (result.Succeeded)
                {
                    return Ok("Account Add Success");
                }
                else
                {
                    return BadRequest(result.Errors.FirstOrDefault());
                }

            }

            return BadRequest(ModelState);

        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (ModelState.IsValid)
            {
            ApplicationUser applicationUser=  await  userManager.FindByEmailAsync(loginDTO.Email);
                if (applicationUser != null)
                {
                    bool result = await userManager.CheckPasswordAsync(applicationUser, loginDTO.Password);
                    if (result)
                    {
                        List<Claim> claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, applicationUser.UserName));
                        claims.Add(new Claim(ClaimTypes.Email, applicationUser.Email));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()));
                        var roles =await userManager.GetRolesAsync(applicationUser);
                        foreach(var item in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, item));
                        }

                        SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecretKey"]));
                        SigningCredentials signingCredentials = new SigningCredentials(algorithm: SecurityAlgorithms.HmacSha256, key: securityKey);
                        //create token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: config["JWT:ValidIssure"],
                            audience: config["JWT:Validaudince"],
                            claims: claims,
                            null,
                           expires: DateTime.Now.AddDays(2),
                            signingCredentials: signingCredentials



                            );
                        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(mytoken), expires = mytoken.ValidTo });
                    }
                    else
                    {
                        return BadRequest("invalid passord");
                    }
                }
                else
                {

                    return BadRequest("invalid name");
                }
            }
            return StatusCode(401);
        }
    }
}
