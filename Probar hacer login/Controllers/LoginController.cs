using Microsoft.AspNetCore.Mvc;
using Probar_hacer_login.Models;
using Probar_hacer_login.Datos;
using System.Data;
using Microsoft.AspNetCore.Authentication;  // necesario para authenticationProperties
using Microsoft.AspNetCore.Authentication.Cookies;  // necesario para cookieAuthentications
using Microsoft.AspNetCore.Authentication.Google;  // necesario para GoogleDefaults
using System.Security.Claims;  // necesario para claimTypes
using System.Threading.Tasks;
using Microsoft.VisualBasic;  // Necesario para task




namespace Probar_hacer_login.Controllers
{
    public class LoginController : Controller
    {

        UsuarioDatos _UsuarioDatos = new UsuarioDatos();
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(UsuarioModels oUsuario)
        {
            if (!ModelState.IsValid)
            {
                return View(oUsuario);
            }
            var respuesta = _UsuarioDatos.Registrar(oUsuario);

            if (respuesta)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ModelState.AddModelError("", "Error al registrar el usuario. El Email podría ya estar en uso o hubo un problema en la base de datos");
                return View(oUsuario);
            }

        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string contraseña)
        {
            var oUsuario = _UsuarioDatos.ValidarCredenciales(email, contraseña);

            if (oUsuario != null)
            {
                return RedirectToAction("index", "home");

            }
            else
            {
                ModelState.AddModelError("", "Credenciales invalidas. Email o contraseñas incorrectos");
                return View();
            }



        }

        // Metodos para la configuracion de autenticacion externa


        // Metodo para iniciar el proceso de autenticacion con google
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleLoginCallback", "Login") };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);

        }

        // Metodo que recibe la respuesta de google despues de la autenticacion
        public async Task<IActionResult> GoogleLoginCallBack()
        {
            try
            {

                // Contiene el resultado de la operación de autenticación que deriva de la cookie.
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);


                // Si el resultado del procesamiento de la autenticacion fue exitoso, es decir, la cookie era valida y contentia una identidad de usuario reconocible, entonces...
                if (authenticateResult.Succeeded)
                {






                    // claims, obtiene los datos del usuario registrado (email, nombre, etc)
                    var claims = authenticateResult.Principal.Claims;

                    // Esta obteniendo precisamente el email
                    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                    var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                    UsuarioModels oUsuario;

                    oUsuario = _UsuarioDatos.ObtenerPorEmail(email);
                    if (oUsuario == null)
                    {
                        oUsuario = new UsuarioModels()
                        {
                            Email = email,
                            Nombre = name,

                        };

                        var registroExitoso = _UsuarioDatos.Registrar(oUsuario);
                        if (!registroExitoso)
                        {
                            return RedirectToAction("Login", "Login");


                        }
                    }
                    var claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties()
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // esta línea redirige al usuario a la página principal de tu aplicación.
                    return RedirectToAction("index", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return RedirectToAction("Login", "Login");
        }
    }



}
