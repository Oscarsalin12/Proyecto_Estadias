using Proyecto_Estadias.Filters;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Security;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Estadias.Controllers
{
    public class AuthController : Controller
    {
        private IClaimManager claimManager;
        private readonly IAuthServices authServices;
        public AuthController()
        {
            claimManager = new ClaimManager();
            authServices = new AuthServices();
        }

        [HttpGet]
        [ExceptionFilterAttribute]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UsuariosVM usuariosVM, string returnUrl)
        {
            if (usuariosVM == null || !ModelState.IsValid)
            {
                ViewBag.Error = "Datos inválidos.";
                return View(usuariosVM);
            }

            var usuarioDb = authServices.login(usuariosVM.Usuario.ToLower(), usuariosVM.Contrasenia.ToLower());
            if (usuarioDb == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(usuariosVM);
            }

            var rolesVM = usuarioDb.UsuarioRol.Select(r => new UsuariosRolVM
            {
                IdRol = r.IdRol,
                IdUsuario = r.IdUsuario,
                RolesVM = new RolVM
                {
                    Id = r.Roles.Id,
                    Nombre = r.Roles.Nombre,
                }
            }).ToList();

            usuariosVM.UsuariosRolesVMs = rolesVM;
            usuariosVM.Id = usuarioDb.Id;
            usuariosVM.Usuario = usuarioDb.Usuario;
            usuariosVM.Contrasenia = usuarioDb.Contrasenia;

            if (claimManager.SignIn(usuariosVM, true, returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Error de autenticación.";
            return View(usuariosVM);
        }

        [HttpGet]
        [ExceptionFilter]
        public ActionResult Account()
        {
            ViewBag.Roles = new
            SelectList(authServices.GetAllRoles(), "Id", "Nombre");
            ViewBag.Empresas = new
            SelectList(authServices.GetAllEmpresas(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ExceptionFilter]
        public ActionResult Account(UsuariosVM usuariosVM)
        {
            System.Diagnostics.Debug.WriteLine("Entró al método POST");

            if (ModelState.IsValid)
            {
                bool creado = authServices.InsertUser(usuariosVM, usuariosVM.Roles, usuariosVM.Empresas);

                if (creado)
                {
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", "No se pudo registrar el usuario. Intente nuevamente.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState inválido");
            }

            ViewBag.Roles = new SelectList(authServices.GetAllRoles(), "Id", "Nombre");
            ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(), "Id", "Nombre");
            return View(usuariosVM);
        }

    }
}
