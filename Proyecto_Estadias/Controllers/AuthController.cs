using Proyecto_Estadias.Filters;
using Proyecto_Estadias.Helpers;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Security;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Estadias.Controllers
{
    public class AuthController : Controller
    {
        private readonly IEmpleadoMedServices empleadoMedServices;
        private readonly IEmpleadosTHHServices empleadoThhServices;
        private readonly IEmpleadosJuarezServices empleadoJuaServices;
        private IClaimManager claimManager;
        private readonly IAuthServices authServices;
        private readonly Vacaciones_ProyectoEntities db = new Vacaciones_ProyectoEntities();
        public AuthController()
        {
            claimManager = new ClaimManager();
            authServices = new AuthServices();
            empleadoJuaServices = new EmpleadoJuarezServices();
            empleadoThhServices = new EmpleadoThhServices();
            empleadoMedServices = new EmpleadosMedServices();
        }

        #region  inicio de sesion 
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
                return RedirectToAction("Perfil", "Auth");
            }

            ViewBag.Error = "Error de autenticación.";
            return View(usuariosVM);
        }

        #endregion

        #region cerrar sesion 
        [HttpGet]
        public ActionResult LogOut()
        {
            claimManager.SignOut();
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region Crear usuarios 
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

        #endregion

        #region perfil

        [Authorize]
        public ActionResult Perfil()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var empresasPermitidas = ClaimsHelper.GetEmpresas(identity);
            var roles = ClaimsHelper.GetRoles(identity);


            var usuarioNombre = User.Identity.Name;
            var usuario = authServices.GetUsuarioConRelaciones(usuarioNombre); 
            if (usuario == null)
            {
                return RedirectToAction("Login");
            }
            var empresas = authServices.GetAllEmpresas();
            var model = new PerfilVM
            {
                Usuario = usuario,
                
                Empresas = empresas
            };
            return View(model);
        }
        #endregion

        #region metodos de lista por empresa 

        [Authorize]
        public ActionResult EmpleadosPorEmpresa(int id)
        {
            var identity = (ClaimsIdentity)User.Identity;

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            //obtener el eol del ususrio actual 

            var roles = ClaimsHelper.GetRoles(identity);
            bool tieneAcceso = roles.Any(rol => ClaimsHelper.TieneAccesoARolYEmpresa(identity, rol, id));

            // se realiza la verificacion 
            if (!tieneAcceso)
            {
                return new HttpUnauthorizedResult("no tiene acceso a esta empresa");
            }

            empleadoJuaServices.ActualizarVacacionesYPermisos();
            empleadoMedServices.ActualizarVacacionesYPermisos();
            empleadoThhServices.ActualizarVacacionesYPermisos();
            var empleados = authServices.GetEmpleadosPorEmpresaYSubempresa(id); 
            ViewBag.EmpresaNombre = authServices.GetEmpresaNombre(id);
            return View(empleados);
        }
        #endregion


        #region metodos de usuarios 
        [HttpGet]
        public ActionResult ListarUsuairos()
        {
            var usuariosVM = authServices.UsuariosAll();
            return View(usuariosVM);
        }

        [HttpGet]
        public ActionResult AsignarRol(int id)
        {

            var roles = authServices.GetAllRoles();
            var empresas = authServices.GetAllEmpresas();

            var model = new AsignarRolVM
            {
                IdUsuario = id,
                ListaRoles = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre }).ToList(),
                ListaEmpresas = empresas.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Nombre}).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult AsignarRol(int idUsuario, int idRol , int idEmpresa)
        {
            bool resultado = authServices.AsignarRolEmpresa(idUsuario, idRol, idEmpresa);

            if (resultado)
                TempData["mensaje"] = "Rol asignado correctamente";
            else
                TempData["error"] = "el ususario ya tiene ese rol en esa empresa";

            return RedirectToAction("Perfil", "Auth");
        }
        #endregion
    }
}
