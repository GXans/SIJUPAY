using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SIJUPAY.Logica;
using SIJUPAY.Models;
using System.Security.Claims;

namespace SIJUPAY.Controllers
{
    public class AccesoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Usuario _usuario)
        {
            Logica_Usuarios usuario = new Logica_Usuarios();
            var n_usuario = usuario.encontrarUsuario(_usuario.Telefono, _usuario.Contrasena);
            if (n_usuario.Telefono != null)
            {
                var ingreso = new List<Claim>
                   {
                    new Claim(ClaimTypes.Name,n_usuario.Nombre),
                    new Claim("Telefono",n_usuario.Telefono)
                   };
                foreach (var rol in n_usuario.TipoUsuario.ToString())
                {
                    ingreso.Add(new Claim(ClaimTypes.Role, rol.ToString()));
                }
                var identificaringreso = new ClaimsIdentity(ingreso,CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identificaringreso));
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario _usuario)
        {
            if (string.IsNullOrEmpty(_usuario.Nombre) || string.IsNullOrEmpty(_usuario.Apellido) || string.IsNullOrEmpty(_usuario.Contrasena) || string.IsNullOrEmpty(_usuario.Telefono))
            {
                ViewData["Mensaje"] = "Debe completar todos los campos requeridos.";
                return View();
            }

            Logica_Usuarios logicaUsuario = new Logica_Usuarios();

            _usuario.TipoUsuario = "Cliente";

            bool registrado = logicaUsuario.registrarUsuario(_usuario);

            if (registrado)
            {
                return RedirectToAction("Index", "Acceso");
            }
            else
            {
                ViewData["Mensaje"] = "Error al registrar el usuario. El número de teléfono podría ya existir.";
                return View();
            }
        }
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Acceso");
        }
    }
}
