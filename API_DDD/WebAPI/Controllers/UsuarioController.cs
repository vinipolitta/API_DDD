using Aplicacao.Interfaces;
using Entidades.Entidades;
using Entidades.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Token;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IAplicacaoUsuario _IAplicacaoUsuario;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public UsuarioController(
            IAplicacaoUsuario IAplicacaoUsuario, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _IAplicacaoUsuario = IAplicacaoUsuario;
            _userManager = userManager;
            _signInManager = signInManager;
            
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("api/CriarToken")]
        public async Task<IActionResult> CriarToken([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))            
                return Unauthorized();

            var resultado = await _IAplicacaoUsuario.ExisteUsuario(login.email, login.senha);
            if (resultado)
            {
                var token = new TokenJWTBuild()
                    .AddSecurityKey(JwTSecurityKey.Create("Secret_key-12345678"))
                 .AddSubject("Empresa - Vinicius Politta")
                 .AddIssuer("Teste.Security.Bearer")
                 .AddAudience("Teste.Security.Bearer")
                 .AddClaim("UsuarioAPINumero", "1")
                 .AddExpiry(5)
                 .Builder();
                return Ok(token.Value);
            }
            else
            {
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("api/AdicionarUsuario")]
        public async Task<IActionResult> AdicionarUsuario([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Ok("Falta alguns dados");

            var resultado = await _IAplicacaoUsuario.AdicionarUsuario(login.email, login.senha, login.idade, login.celular);

            if (resultado)
            {
                return Ok("Usuario Adicionado com sucesso!!!");
            }
            else
            {
                return Ok("Erro ao adicionar o usuario");
            }
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("api/CriarTokenIdentity")]
        public async Task<IActionResult> CriarTokenIdentity([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Ok("Falta alguns dados");

            var resultado = await _signInManager.PasswordSignInAsync(login.email, login.senha, false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var token = new TokenJWTBuild()
                    .AddSecurityKey(JwTSecurityKey.Create("Secret_key-12345678"))
                 .AddSubject("Empresa - Vinicius Politta")
                 .AddIssuer("Teste.Security.Bearer")
                 .AddAudience("Teste.Security.Bearer")
                 .AddClaim("UsuarioAPINumero", "1")
                 .AddExpiry(5)
                 .Builder();
                return Ok(token.Value);
            }
            else
            {
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("api/AdicionarUsuarioIdentity")]
        public async Task<IActionResult> AdicionarUsuarioIdentity([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Ok("Falta alguns dados");

            var user = new ApplicationUser
            {
                UserName = login.email,
                Email = login.senha,
                Celular = login.celular,
                Tipo = TipoUsuario.Comun
            };

            var resultado = await _userManager.CreateAsync(user, login.senha);
            if (resultado.Errors.Any())
            {
                return Ok(resultado.Errors);
            }

            //Geracao de confirmacao caso precise
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            //retorno email
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var resultado2 = await _userManager.ConfirmEmailAsync(user, code);
            var StatusMessage = resultado2.Succeeded;

            if (resultado2.Succeeded)
                return Ok("Usuario Adicionado COM SUCESSO");
            else
                return Ok("ERRO AO CONFIRMAR O USUARIO");
        }
    }
}
