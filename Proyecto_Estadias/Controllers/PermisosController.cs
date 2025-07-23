using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
using System.Security.Claims;
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

        #region metodo para crear permisos Empleados conpletos 

        [HttpGet]

        public ActionResult CrearPermiso(int? idEmpleado = null)
        {
            var model = new PermisosVM
            {
                Empleados = permisosServices.ObtenerEmpleados(),
                MotivosAusencia = permisosServices.ObtenerMotivos(),
                SubEmpresas = permisosServices.ObtenerEmpresas(),
                FechaGeneracion = DateTime.Today,
               
            };

            if (idEmpleado.HasValue)
            {
                var empleado = permisosServices.ObtenerEmpleadoPorId(idEmpleado.Value);

                model.IdEmpleado = empleado.Id;
                model.IdEmpresa = empleado.IdEmpresa;
                model.NombreEmpleado = empleado.Nombre;
                model.EmpresaNombre = empleado.SubEmpresas.Nombre;
                model.VacacionesDisponibles = empleado.DiasDisponibles;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult CrearPermiso(PermisosVM model)
        {
            if (ModelState.IsValid)
            {

                if (permisosServices.CrearPermiso(model, out string error))
                {
                    TempData["PermisoCreado"] = true;
                    return RedirectToAction("ListaEmpleadosSubempresa", "Auth", new { idSub = model.IdEmpresa });
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
            model.Empleados = permisosServices.ObtenerEmpleados();
            model.MotivosAusencia = permisosServices.ObtenerMotivos();
            model.SubEmpresas = permisosServices.ObtenerEmpresas();
            return View(model);
        }


        public JsonResult BuscarEmpleado(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Json(null, JsonRequestBehavior.AllowGet);

            var empleado = db.Empleados.Include("SubEmpresas")
                .FirstOrDefault(e => e.Nombre.Contains(filtro) || e.NumEmpleado.ToString() == filtro);

            if (empleado == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                id = empleado.Id,
                nombre = empleado.Nombre,
                numEmpleado = empleado.NumEmpleado,
                empresa = empleado.SubEmpresas?.Nombre ?? "",
                empresaId = empleado.IdEmpresa,
                vacaciones = empleado.DiasDisponibles
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region metodo para permisos para empleado individual 


   
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
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fuente personalizada
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var subTituloFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var encabezadoFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var celdaFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);

                // Título principal
                Paragraph titulo = new Paragraph("Reporte de Faltas", tituloFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                doc.Add(titulo);

                // Fecha
                Paragraph fecha = new Paragraph("Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy"), subTituloFont);
                fecha.Alignment = Element.ALIGN_RIGHT;
                fecha.SpacingAfter = 20f;
                doc.Add(fecha);

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2.5f, 2.5f, 2.5f, 2f });

                // Encabezados
                BaseColor headerBackground = new BaseColor(0, 122, 204); // azul suave
                string[] headers = { "Empresa", "Empleado", "Motivo", "Fecha" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, encabezadoFont));
                    cell.BackgroundColor = headerBackground;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Celdas de contenido
                foreach (var p in permisos)
                {
                    table.AddCell(new PdfPCell(new Phrase(p.SubEmpresas?.Nombre ?? "N/A", celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.Empleados?.Nombre ?? "N/A", celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.MotivosAusencia?.NombreMotivo ?? "N/A", celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaGeneracion.ToString("dd/MM/yyyy"), celdaFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "ReporteDeFaltas.pdf");
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
                    empresa = empleado.SubEmpresas.Nombre ?? "No asignada",
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
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fuentes
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var subTituloFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);

                // Título
                Paragraph titulo = new Paragraph("Reporte Individual de Permisos", tituloFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20f;
                doc.Add(titulo);

                // Datos del empleado
                doc.Add(new Paragraph($"Empleado: {empleado.Nombre}", subTituloFont));
                doc.Add(new Paragraph($"Empresa: {empleado.SubEmpresas?.Nombre ?? "N/A"}", subTituloFont));
                doc.Add(new Paragraph($"Días de vacaciones otorgados: {empleado.intDiasVacaciones}", subTituloFont));
                doc.Add(new Paragraph($"Días disponibles: {empleado.DiasDisponibles}", subTituloFont));
                doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}", subTituloFont));
                doc.Add(new Paragraph(" ")); // Espacio

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 3f, 2f, 2f });

                // Encabezados
                BaseColor azulEncabezado = new BaseColor(0, 122, 204);
                string[] headers = { "Fecha Generación", "Motivo", "Inicio", "Final" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = azulEncabezado;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Filas de contenido
                foreach (var p in permisos)
                {
                    table.AddCell(new PdfPCell(new Phrase(p.FechaGeneracion.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.MotivosAusencia?.NombreMotivo ?? "N/A", cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaInicio.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaFinal.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
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
            List<Permisos> resultado = permisosServices.PermisosAll()
                .OrderBy(p => p.FechaGeneracion).ToList(); //Ordenamiento por fecha 
            //Se aplica paginacon paginacion
            List<PermisosPdfVM> permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            return View(permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }

        #endregion

        public FileResult GenerarPDFPermiso(int idPermiso)
        {
            var permiso = permisosServices.ObtenerPermisoPorId(idPermiso);
            var empleado = permiso.Empleados;

            var empresa = permiso.SubEmpresas?.Empresas?.Nombre ?? permiso.SubEmpresas?.Nombre ?? "N/A";
            var motivo = permiso.MotivosAusencia?.NombreMotivo ?? "N/A";

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);

                Paragraph header = new Paragraph(empresa.ToUpper(), titleFont);
                header.Alignment = Element.ALIGN_CENTER;
                doc.Add(header);

                Paragraph subHeader = new Paragraph("AVISO DE AUSENCIA", boldFont);
                subHeader.Alignment = Element.ALIGN_CENTER;
                subHeader.SpacingAfter = 15f;
                doc.Add(subHeader);

                PdfPTable linea1 = new PdfPTable(3) { WidthPercentage = 100 };
                linea1.SetWidths(new float[] { 1f, 1f, 1f });
                linea1.AddCell(CreateCell($"No. Folio.: {permiso.Id}", normalFont));
                linea1.AddCell(CreateCell($"No. Trab.: {empleado.NumEmpleado}", normalFont));
                linea1.AddCell(CreateCell($"Fecha Solicitud: {permiso.FechaGeneracion:dd-MMMM-yyyy}", normalFont));
                doc.Add(linea1);

                doc.Add(CreateLabelLine("NOMBRE:", empleado.Nombre, normalFont));

                PdfPTable linea2 = new PdfPTable(2) { WidthPercentage = 100 };
                linea2.SetWidths(new float[] { 1f, 1f });
                linea2.AddCell(CreateLabelLine("DEPARTAMENTO:", empleado.Departamentos?.NombreDep ?? "N/A", normalFont));
                linea2.AddCell(CreateLabelLine("ENCARGADO:", empleado.Departamentos?.NombreEmcargado ?? "N/A", normalFont));
                doc.Add(linea2);

                doc.Add(CreateLabelLine("MOTIVO:", motivo, normalFont));

                // ✅ Días hábiles según el tipo de semana del empleado
                int diasLaborales = empleado.DiasLaboralesPorSemana > 0 ? empleado.DiasLaboralesPorSemana : 5;
                int totalDias = ContarDiasHabiles(permiso.FechaInicio, permiso.FechaFinal, diasLaborales);

                doc.Add(CreateLabelLine("DIAS:", totalDias.ToString(), normalFont));

                PdfPTable linea3 = new PdfPTable(2) { WidthPercentage = 100 };
                linea3.SetWidths(new float[] { 1f, 1f });
                linea3.AddCell(CreateLabelLine("ESTARÁ AUSENTE A PARTIR DEL:", permiso.FechaInicio.ToString("dd-MMMM-yyyy"), normalFont));
                linea3.AddCell(CreateLabelLine("AL:", permiso.FechaFinal.ToString("dd-MMMM-yyyy"), normalFont));
                doc.Add(linea3);

                doc.Add(CreateLabelLine("OBS.:", $"Restan {permiso.DiasDisponibles} días de vacaciones periodo {DateTime.Now.Year}", normalFont));

                doc.Add(new Paragraph("\n\n\n"));

                PdfPTable firmas = new PdfPTable(3) { WidthPercentage = 100 };
                firmas.AddCell(CreateSignatureCell("RECURSOS HUMANOS"));
                firmas.AddCell(CreateSignatureCell("ENC. DEPARTAMENTO"));
                firmas.AddCell(CreateSignatureCell("INTERESADO"));
                doc.Add(firmas);

                doc.Close();
                return File(ms.ToArray(), "application/pdf", $"AvisoPermiso_{empleado.Nombre}.pdf");
            }
        }



        // Métodos de apoyo
        private PdfPCell CreateCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f };
        }

        private PdfPTable CreateLabelLine(string label, string value, Font font)
        {
            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 0.25f, 0.75f });
            table.AddCell(new PdfPCell(new Phrase(label, font)) { Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase(value, font)) { Border = Rectangle.NO_BORDER });
            return table;
        }

        private PdfPCell CreateSignatureCell(string label)
        {
            var cell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 20f
            };
            cell.Phrase = new Phrase(label, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10));
            return cell;
        }

        private int ContarDiasHabiles(DateTime fechaInicio, DateTime fechaFin, int diasLaboralesPorSemana = 5)
        {
            int total = 0;
            for (var date = fechaInicio.Date; date <= fechaFin.Date; date = date.AddDays(1))
            {
                if (diasLaboralesPorSemana == 6)
                {
                    if (date.DayOfWeek != DayOfWeek.Sunday)
                        total++;
                }
                else
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                        total++;
                }
            }
            return total;
        }



        [HttpGet]
        public ActionResult PaginacionPorEmpresa(int idEmpresa, int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);

            List<Permisos> resultado = permisosServices.PermisosPorEmpresa(idEmpresa)
                .OrderBy(p => p.FechaGeneracion)
                .ToList();

            List<PermisosPdfVM> permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            return View("Paginacion", permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }

    }
}

