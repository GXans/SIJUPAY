using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SIJUPAY.Logica;
using SIJUPAY.Models;
using System.Security.Claims;

namespace SIJUPAY.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Logica_Usuarios logicaUsuario = new Logica_Usuarios();

            string telefonoUsuario = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Telefono")?.Value;

            Usuario usuarioLogueado = logicaUsuario.obtenerUsuarioPorTelefono(telefonoUsuario);

            return View(usuarioLogueado);
        }
    }
}