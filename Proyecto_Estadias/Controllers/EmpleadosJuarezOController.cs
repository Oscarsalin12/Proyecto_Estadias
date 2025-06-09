using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Estadias.Controllers
{
    public class EmpleadosJuarezOController : Controller
    {
        private readonly IEmpleadosJuarezServices empleadoServices;
        private readonly IAuthServices authServices;
        public EmpleadosJuarezOController()
        {
            authServices = new AuthServices();
            empleadoServices = new EmpleadoJuarezServices();
        }

        #region metodos para insertar empleado


        [HttpGet]
        public ActionResult InsertarEmpleado()
        {
            var empresas = authServices.ObtenerSubEmpresasJuarez();
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");
            return View();
        }



        [HttpPost]
        public ActionResult InsertarEmpleado(EmpleadosJuarezO empleado)
        {
            if (ModelState.IsValid)
            {
                empleadoServices.InsertarEmpleados(empleado);
                return RedirectToAction("Perfil", "Auth");
            }
            var empresas = authServices.ObtenerSubEmpresasJuarez();
            ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");
            return View(empleado);

        }

        #endregion


        #region metodos de actualizacion 
        [HttpGet]
        public ActionResult EditarEmpleado(int id)
        {
            var empleado = empleadoServices.GetEmpleadoPorId(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }

            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEmpleado(EmpleadosJuarezO emp)
        {
            if (ModelState.IsValid)
            {
                empleadoServices.ActualizarEmpleado(emp); // Aquí se hace el cálculo y el update
                return RedirectToAction("Perfil", "Auth"); // Ajusta el nombre si es necesario
            }

            return View(emp);
        }

        #endregion

    }
}
