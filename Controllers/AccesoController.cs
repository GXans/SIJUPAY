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
                    new Claim("Telefono",n_usuario.Telefono),
                    new Claim(ClaimTypes.Role, n_usuario.TipoUsuario)
                   };
                
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
            Logica_Usuarios logicaUsuario = new Logica_Usuarios();

            // 1. VALIDACIÓN BÁSICA DE CAMPOS (ya existía)
            if (string.IsNullOrEmpty(_usuario.Nombre) || string.IsNullOrEmpty(_usuario.Apellido) || string.IsNullOrEmpty(_usuario.Contrasena) || string.IsNullOrEmpty(_usuario.Telefono))
            {
                ViewData["Mensaje"] = "Debe completar todos los campos requeridos.";
                return View();
            }

            if (logicaUsuario.existeTelefono(_usuario.Telefono))
            {
                ViewData["Mensaje"] = "El número de teléfono ya se encuentra registrado. Por favor, inicie sesión o use otro número.";
                return View(); // Regresa a la vista con el mensaje de error
            }


            _usuario.TipoUsuario = "Cliente";

            bool registrado = logicaUsuario.registrarUsuario(_usuario);

            if (registrado)
            {
                return RedirectToAction("Index", "Acceso");
            }
            else
            {
                ViewData["Mensaje"] = " Error inesperado al intentar registrar el usuario. Intente más tarde.";
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
