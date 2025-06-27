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
    public class EmpresasController : Controller
    {
        private readonly IEmpresasService empresasService;
        public EmpresasController()
        {
            empresasService = new EmpresasService();
        }
        [HttpGet]
        public ActionResult InsertarEmpresa()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InsertarEmpresa(Empresas empresa, HttpPostedFileBase archivoLogo)
        {
            if (ModelState.IsValid)
            {
                empresasService.InsertarEmpresa(empresa, archivoLogo);
                return RedirectToAction("Login", "Auth");  
            }
            return View(empresa);
        }
    }
}