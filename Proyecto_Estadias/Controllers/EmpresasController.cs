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
        private readonly IAuthServices authServices;
        public EmpresasController()
        {
            empresasService = new EmpresasService();
            authServices = new AuthServices();
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
                return RedirectToAction("Perfil", "Auth");  
            }
            return View(empresa);
        }

        [HttpGet]
        public ActionResult InsertarSubEmpresa()
        {
            ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(),"Id","Nombre");
            return View();
        }

        [HttpPost]
        public ActionResult InsertarSubEmpresa(SubEmpresas subEmpresas, HttpPostedFileBase archivoLogo)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(), "Id", "Nombre");
                return View(subEmpresas);
            }

           empresasService.InsertarSubEmpresas(subEmpresas, archivoLogo);
           return RedirectToAction("Perfil", "Auth");
        }
    }
}