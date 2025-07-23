using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Proyecto_Estadias.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadosServices empleadoServices;
        private readonly IAuthServices authServices;
        public EmpleadosController() 
        {
            authServices = new AuthServices();
            empleadoServices = new EmpleadoServices();
        }

        #region metodos para insertar empleado


        [HttpGet]
        public ActionResult InsertarEmpleado(int idsub)
        {
        
            var empresas = authServices.ObtenerSubEmpresas();
            var departamentos = authServices.GetDepartamentos();
            ViewBag.Departamentos = new SelectList(departamentos, "Id", "NombreDep");
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");

            var nuevoEmpleado = new Empleados
            {
                IdEmpresa = idsub,
                FechaIDeIngreso = DateTime.Now
            };
            return View(nuevoEmpleado);
        }



        [HttpPost]
        public ActionResult InsertarEmpleado(Empleados empleado)
        {
            if (ModelState.IsValid)
            {
                empleadoServices.InsertarEmpleados(empleado);
                TempData["EmpleadoCreado"] = true;
                return RedirectToAction("ListaEmpleadosSubempresa", "Auth", new { idSub = empleado.IdEmpresa });
            }
            var empresas = authServices.ObtenerSubEmpresas();
            var departamentos = authServices.GetDepartamentos();
            ViewBag.Departamentos = new SelectList(departamentos, "Id", "NombreDep");
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");
            return View(empleado);

        }

        #endregion


        #region metodos de actualizacion 
        [HttpGet]
        public ActionResult EditarEmpleado(int id)
        {
            var empleado = empleadoServices.ObtenerEmpleadoPorId(id);
            if (empleado == null)
            {
             TempData["ErrorEmpleado"] = true;
                return HttpNotFound();
            }

            var empresas = authServices.ObtenerSubEmpresas();
            var departamentos = authServices.GetDepartamentos();
            ViewBag.Departamentos = new SelectList(departamentos, "Id", "NombreDep");
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");

            return View(empleado);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEmpleado(Empleados empleado)
        {
            if (ModelState.IsValid)
            {
                empleadoServices.ActualizarEmpleado(empleado);
                TempData["EmpleadoEditado"] = true;
                return RedirectToAction("ListaEmpleadosSubempresa","Auth", new { idSub = empleado.IdEmpresa });


            }

            var empresas = authServices.ObtenerSubEmpresas();
            var departamentos = authServices.GetDepartamentos();
            ViewBag.Departamentos = new SelectList(departamentos, "Id", "NombreDep");
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");

            return View(empleado);
        }


        #endregion



    }
}

