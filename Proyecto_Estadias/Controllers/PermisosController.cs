using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using PagedList;
namespace Proyecto_Estadias.Controllers
{
    public class PermisosController : Controller
    {
        private readonly Vacaciones_ProyectoEntities db = new Vacaciones_ProyectoEntities();
        private readonly IPermisosServices permisosServices = null;

        public PermisosController()
        {
            this.permisosServices = new PermisosService();
        }

        #region metodo para crear permisos

        [HttpGet]
        public ActionResult CrearPermiso()
        {
            var model = new PermisosVM
            {
                Empleados = permisosServices.ObtenerEmpleados(),
                MotivosAusencia = permisosServices.ObtenerMotivos(),
                Empresas = permisosServices.ObtenerEmpresas(),
                FechaGeneracion = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CrearPermiso(PermisosVM model)
        {
            if (ModelState.IsValid)
            {
                var permiso = new Permisos
                {
                    IdEmpleado = model.IdEmpleado,
                    IdTipoPermiso = model.IdTipoPermiso,
                    IdEmpresa = model.IdEmpresa,
                    FechaGeneracion = DateTime.Now,
                    FechaInicio = model.FechaInicio,
                    FechaFinal = model.FechaFinal

                };
                if (permisosServices.CrearPermiso(permiso, out string error))
                {
                    return RedirectToAction("Paginacion");
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
            model.Empleados = permisosServices.ObtenerEmpleados();
            model.MotivosAusencia = permisosServices.ObtenerMotivos();
            model.Empresas = permisosServices.ObtenerEmpresas();
            return View(model);
        }

        #endregion


        // no aplica 
        //public ActionResult Reporte()
        //{
        //    var permisos = db.Permisos.Include("Empresas").Include("Empleados").Include("MotivosAusencia").ToList();
        //    return View(permisos);
        //}

        #region generar pdf
        public FileResult GenerarPDF()
        {
            var permisos = permisosServices.ObtenerPermisosConDetalles();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                doc.Add(new Paragraph("Reporte de faltas"));
                doc.Add(new Paragraph("Fecha: " + DateTime.Now.ToShortDateString()));
                doc.Add(new Paragraph(""));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Empresas");
                table.AddCell("Empleado");
                table.AddCell("Motivo");
                table.AddCell("Fecha");

                foreach (var p in permisos)
                {
                    table.AddCell(p.Empresas?.Nombre ?? "N/A");
                    table.AddCell(p.Empleados?.Nombre ?? "N/A");
                    table.AddCell(p.MotivosAusencia?.NombreMotivo ?? "N/A");
                    table.AddCell(p.FechaGeneracion.ToShortDateString());

                }
                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "Reportes.pdf");
            }
        }

        [HttpGet]
        public JsonResult ObtenerDatosEmpleado(int id)
        {
            var empleado = permisosServices.ObtenerEmpleadoPorId(id);
            if (empleado != null)
            {

                return Json(new
                {
                    empresaId = empleado.IdEmpresa,
                    empresa = empleado.Empresas.Nombre ?? "No asignada",
                    vacaciones = empleado.DiasDisponibles
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new {
                empresaId = 0,
                empresa = "No encontrada",
                vacaciones = 0 },
                JsonRequestBehavior.AllowGet);
        }

        public FileResult  GenerarPDFEmpleado(int id)
        {
            var data = permisosServices.ObtenerPermisosPorEmpleados(id);
            var empleado = data.Item1;
            var permisos = data.Item2;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                doc.Add(new Paragraph($"Reporte indiviual de permisos para: {empleado.Nombre}"));
                doc.Add(new Paragraph($"que forma parte de la Empresa : {empleado.Empresas?.Nombre}"));
                doc.Add(new Paragraph($"Con derecho a : {empleado.intDiasVacaciones} de vacaciones"));
                doc.Add(new Paragraph($"Con dias disponibles : {empleado.DiasDisponibles}"));
                doc.Add(new Paragraph(""));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Fecha Generacion");
                table.AddCell("Motivo");
                table.AddCell("Inicio");
                table.AddCell("Final");

                foreach (var p in permisos)
                {
                    table.AddCell(p.FechaGeneracion.ToShortDateString());
                    table.AddCell(p.MotivosAusencia?.NombreMotivo ?? "N/A");
                    table.AddCell(p.FechaInicio.ToShortDateString());
                    table.AddCell(p.FechaFinal.ToShortDateString());
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"ReporteEmpleado_{empleado.Nombre}.pdf");
            }
        }

        #endregion



        [HttpGet]
        public ActionResult Paginacion(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1); //Pagina por defecto 
            List<Permisos> resultado = permisosServices.PermisosAll()
                .OrderBy(p => p.FechaGeneracion).ToList(); //Ordenamiento por fecha 
            //Se aplica paginacon paginacion
            List<PermisosPdfVM> permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            return View(permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }


        public JsonResult BuscarEmpleado(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Json(null, JsonRequestBehavior.AllowGet);

            var empleado = db.Empleados.Include("Empresas")
                .FirstOrDefault(e => e.Nombre.Contains(filtro) || e.NumEmpleado.ToString() == filtro);

            if (empleado == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                id = empleado.Id,
                empresa = empleado.Empresas?.Nombre ?? "",
                empresaId = empleado.IdEmpresa,
                vacaciones = empleado.DiasDisponibles
            }, JsonRequestBehavior.AllowGet);
        }

    } 
}

