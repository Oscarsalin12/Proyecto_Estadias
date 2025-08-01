
using PagedList;
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
using PagedList.Mvc;

namespace Proyecto_Estadias.Controllers
{
    public class AuthController : Controller
    {
       
        private readonly IEmpleadosServices empleadoServices;
      
        private IClaimManager claimManager;
        private readonly IAuthServices authServices;
        private readonly Vacaciones_ProyectoEntities db = new Vacaciones_ProyectoEntities();
        public AuthController()
        {
            claimManager = new ClaimManager();
            authServices = new AuthServices();
            empleadoServices = new EmpleadoServices();
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
        public ActionResult Login(UsuariosVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (claimManager.SignIn(model, true, returnUrl))
                {
                    TempData["LoginExitoso"] = true;
                    return Redirect(returnUrl ?? Url.Action("Perfil", "Auth"));
                }
                ModelState.AddModelError("", "Credenciales incorrectas.");
            }
            return View(model);
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
                    TempData["UsuarioCreado"] = true;
                    return RedirectToAction("Perfil");
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

        //[Authorize]
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
        public ActionResult SeleccionarSubempresa(int id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var roles = ClaimsHelper.GetRoles(identity);
            bool tieneAcceso = roles.Any(rol => ClaimsHelper.TieneAccesoARolYEmpresa(identity, rol, id));
           
            // se realiza la verificacion
            if (!tieneAcceso)
                return new HttpUnauthorizedResult("no tiene acceso a esta empresa");

            var subempresas = authServices.GetSubEmpresasPorEmpresa(id);
            if(subempresas.Any())
            {
                ViewBag.EmpresaMadre = db.Empresas.Where(e => e.Id == id).Select(e => e.Nombre).FirstOrDefault();
                ViewBag.EmpresaId = id;
                ViewBag.SubEmpresaId = id; 
                return View("SeleccionarSubempresa", subempresas);

            }
            return View("SeleccionarSubempresa", subempresas);
            //return RedirectToAction("ListaEmpleadosSubempresa", new { idSub = id });

        }


        public ActionResult ListaEmpleadosSubempresa(int idSub, int? bus, string busqueda, int? page, int pageSize = 5)
        {
          
            if (idSub <= 0)
            {
                return HttpNotFound("ID de subempresa no válido");
            }


            empleadoServices.ActualizarVacacionesYPermisos();

            var empleadosQuery = db.Empleados
                .Include("SubEmpresas")
                .Include("Departamentos")
                .Where(e => e.IdEmpresa == idSub);

            if (!String.IsNullOrEmpty(busqueda))
            {
                empleadosQuery = empleadosQuery.Where(e => e.Nombre.Contains(busqueda));
            }
            
            if (bus.HasValue && bus.Value > 0)
            {
                empleadosQuery = empleadosQuery.Where(e => e.IdDepartamento == bus.Value);
            }

            var empleadosVM = empleadosQuery.Select(e => new EmpleadosVM
            {
                Id = e.Id,
                NumEmpleado = e.NumEmpleado,
                Nombre = e.Nombre,
                FechaDeIngreso = e.FechaIDeIngreso,
                AniosTrabajados = e.AniosTrabajados,
                MesesTrabajados = e.MesesTrabajados,
                DiasLaboralesPorSemana = e.DiasLaboralesPorSemana,
                intDiasVacaciones = e.intDiasVacaciones,
                DiasDisponibles = e.DiasDisponibles,
                IdEmpresa = e.IdEmpresa,
                IdDepartamento = e.IdDepartamento,
                NombreEmpresa = e.SubEmpresas.Nombre,
                NombreDepartamento = e.Departamentos.NombreDep
            })
                .OrderBy(e => e.Nombre)
            .ToPagedList(page ?? 1, pageSize);

            ViewBag.SubempresaNombre = authServices.GetNombreSubempresas(idSub);
            ViewBag.IdSub = idSub;

            ViewBag.Departamentos = new SelectList(db.Departamentos, "Id", "NombreDep");

            return View("EmpleadosPorEmpresa", empleadosVM);
        }



        #endregion


        #region metodos de usuarios 
        [HttpGet]
        [Authorize(Roles = "AdministadorGeneral")]
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
                ListaEmpresas = empresas.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Nombre }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult AsignarRol(int idUsuario, int idRol, int idEmpresa)
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
    

