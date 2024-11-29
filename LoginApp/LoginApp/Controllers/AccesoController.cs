using Microsoft.AspNetCore.Mvc;

using LoginApp.Data;
using LoginApp.Models;
using Microsoft.EntityFrameworkCore;
using LoginApp.ViewModels;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LoginApp.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public AccesoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            if(modelo.Clave != modelo.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                NombreCompleto = modelo.NombreCompleto,
                Correo = modelo.Correo,
                Clave = modelo.Clave
            };

            await _appDBContext.Usuarios.AddAsync(usuario);
            await _appDBContext.SaveChangesAsync();

            if(usuario.IdUsuario != 0) return RedirectToAction("Login", "Acceso");

            ViewData["Mensaje"] = "No se pudo crear el usuario, fatal error";
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            Usuario? usuario_encontrado = await _appDBContext.Usuarios
                                                .Where(u => 
                                                u.Correo == modelo.Correo && 
                                                u.Clave == modelo.Clave)
                                                .FirstOrDefaultAsync();

            if(usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "Correo o usuario erroneos, valide nuevamente";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario_encontrado.NombreCompleto)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties() 
            {
                AllowRefresh= true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
                );

            return RedirectToAction("Index", "Home");
        }
    }
}
