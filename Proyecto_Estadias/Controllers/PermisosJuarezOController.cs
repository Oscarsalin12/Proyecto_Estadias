using AutoMapper;
using iTextSharp.text.pdf;
using iTextSharp.text;
using PagedList;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Estadias.Controllers
{
    public class PermisosJuarezOController : Controller
    {
        private readonly Vacaciones_ProyectoEntities db = new Vacaciones_ProyectoEntities();
        private readonly IPermisosJuarezOServices permisosServices = null;

        public PermisosJuarezOController()
        {
            this.permisosServices = new PermisosJuarezOServices();
        }

        #region metodo para crear permisos

        [HttpGet]

        public ActionResult CrearPermiso()
        {
            var model = new PermisosVM
            {
                EmpleadosJuarezO = permisosServices.ObtenerEmpleados(),
                MotivosAusencia = permisosServices.ObtenerMotivos(),
                SubEmpresasJuarezOs = permisosServices.ObtenerEmpresas(),
                FechaGeneracion = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CrearPermiso(PermisosVM model)
        {
            if (ModelState.IsValid)
            {
                var permiso = new PermisosJuarezO
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
            model.EmpleadosJuarezO = permisosServices.ObtenerEmpleados();
            model.MotivosAusencia = permisosServices.ObtenerMotivos();
            model.SubEmpresasJuarezOs = permisosServices.ObtenerEmpresas();
            return View(model);
        }


        public JsonResult BuscarEmpleado(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Json(null, JsonRequestBehavior.AllowGet);

            var empleado = db.EmpleadosTHH.Include("SubEmpresasTHH")
                .FirstOrDefault(e => e.Nombre.Contains(filtro) || e.NumEmpleado.ToString() == filtro);

            if (empleado == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                id = empleado.Id,
                nombre = empleado.Nombre,
                numEmpleado = empleado.NumEmpleado,
                empresa = empleado.SubEmpresasTHH?.Nombre ?? "",
                empresaId = empleado.IdEmpresa,
                vacaciones = empleado.DiasDisponibles
            }, JsonRequestBehavior.AllowGet);
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
                    table.AddCell(p.SubEmpresasJuarezO?.Nombre ?? "N/A");
                    table.AddCell(p.EmpleadosJuarezO?.Nombre ?? "N/A");
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
                    empresa = empleado.SubEmpresasJuarezO.Nombre ?? "No asignada",
                    vacaciones = empleado.DiasDisponibles
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                empresaId = 0,
                empresa = "No encontrada",
                vacaciones = 0
            },
                JsonRequestBehavior.AllowGet);
        }

        public FileResult GenerarPDFEmpleado(int id)
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
                doc.Add(new Paragraph($"que forma parte de la Empresa : {empleado.SubEmpresasJuarezO?.Nombre}"));
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


        #region listado de permisos generales 
        [HttpGet]
        public ActionResult Paginacion(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1); //Pagina por defecto 
            List<PermisosJuarezO> resultado = permisosServices.PermisosAll()
                .OrderBy(p => p.FechaGeneracion).ToList(); //Ordenamiento por fecha 
            //Se aplica paginacon paginacion
            List<PermisosPdfVM> permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            return View(permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }

        #endregion

    }
}