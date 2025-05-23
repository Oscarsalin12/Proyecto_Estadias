using iTextSharp.text;
using iTextSharp.text.pdf;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                EmpleadosVM  = db.Empleados.Select(e => new SelectListItem
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
               
                };

                db.Permisos.Add(permiso);
                //Descontar un dia de vacaciones 
                var empleado = db.Empleados.FirstOrDefault(e => e.Id == model.IdEmpleado);
                if(empleado != null && empleado.intDiasVacaciones >0)
                {
                    empleado.intDiasVacaciones -= 1;
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

            using(MemoryStream ms = new MemoryStream())
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

                foreach ( var p in permisos)
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




    }
}