using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Proyecto_Estadias.Controllers
{
    public class PermisosController : Controller
    {
        private readonly Vacaciones_ProyectoEntities db = new Vacaciones_ProyectoEntities();


        [HttpGet]
        public ActionResult CrearPermiso()
        {
            var model = new PermisosVM
            {
                EmpleadosVM = db.Empleados.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre
                }).ToList(),

                MotivosAusenciaVM = db.MotivosAusencia.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.NombreMotivo
                }).ToList(),
                FechaGeneracion = DateTime.Today
            };
            model.EmpresasVM = db.Empresas.Select(em => new SelectListItem
            {
                Value = em.Id.ToString(),
                Text = em.Nombre
            }).ToList();
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

                db.Permisos.Add(permiso);
                //Descontar un dia de vacaciones 
                var empleado = db.Empleados.FirstOrDefault(e => e.Id == model.IdEmpleado);
                if (empleado != null)
                {
                    int diasSolicitados = (model.FechaFinal - model.FechaInicio).Days + 1;

                    if (empleado.DiasDisponibles >= diasSolicitados)
                    {
                        empleado.DiasDisponibles -= diasSolicitados;
                    }
                    else
                    {
                        ModelState.AddModelError("", "El empleado no tiene suficientes dias disponibles.");
                        return View(model);
                    }
                } else
                {
                    ModelState.AddModelError("", "La fecha final no puede ser menor que la fecha de inicio");
                    return View(model);
                }
                
                db.SaveChanges();
                return RedirectToAction("Reporte");
            }

            model.EmpleadosVM = db.Empleados.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Nombre
            }).ToList();

            model.MotivosAusenciaVM = db.MotivosAusencia.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.NombreMotivo
            }).ToList();

            model.EmpresasVM = db.Empresas.Select(em => new SelectListItem
            {
                Value = em.Id.ToString(),
                Text = em.Nombre
            }).ToList();

            return View(model);
        }

        public ActionResult Reporte()
        {
            var permisos = db.Permisos.Include("Empresas").Include("Empleados").Include("MotivosAusencia").ToList();
            return View(permisos);
        }

        public FileResult GenerarPDF()
        {
            var permisos = db.Permisos.Include("Empresas").Include("Empleados").Include("MotivosAusencia").ToList();

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
            var empleado = db.Empleados.Include("Empresas").FirstOrDefault(e => e.Id == id);
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
            var empleado = db.Empleados.FirstOrDefault(e => e.Id == id);
            var permisos = db.Permisos
                .Where(p => p.IdEmpleado == id)
                .Include("Empresas")
                .Include("MotivosAusencia")
                .ToList();

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
    } 
}

