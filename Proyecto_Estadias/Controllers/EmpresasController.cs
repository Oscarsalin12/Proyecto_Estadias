using Org.BouncyCastle.Crypto;
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
                TempData["EmpresaCreada"] = true;
                return RedirectToAction("Perfil", "Auth");  
            }
            return View(empresa);
        }

        [HttpGet]
        public ActionResult InsertarSubEmpresa(int? idEmpresaMadre)
        {
            ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(), "Id", "Nombre", idEmpresaMadre);
           
            var model = new SubEmpresas();

            if (idEmpresaMadre.HasValue)
            {
                model.IdEmpresa_madre = idEmpresaMadre.Value;
                         
            }
            ViewBag.IdSub = idEmpresaMadre;
         
            return View(model);
        }


        [HttpPost]
        public ActionResult InsertarSubEmpresa(SubEmpresas subEmpresas, HttpPostedFileBase archivoLogo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(), "Id", "Nombre");
                    TempData["ErrorIner"] = true;
                    return View(subEmpresas);
                }

                empresasService.InsertarSubEmpresas(subEmpresas, archivoLogo);

                TempData["ExitoSub"] = true;
                return RedirectToAction("Perfil", "Auth");
            }
            catch (Exception ex)
            {
                TempData["Error404"] = "Ocurrió un error inesperado: " + ex.Message;
                ViewBag.Empresas = new SelectList(authServices.GetAllEmpresas(), "Id", "Nombre");
                return View(subEmpresas);
            }
        }

    }
}