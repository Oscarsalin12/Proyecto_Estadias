using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.Services;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

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
            ViewBag.EmpresaId = model.IdEmpresa;
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

        [HttpGet]
        public ActionResult SolicitarPermiso(int? idEmpleado)
        {
            var model = new PermisosVM
            {
                Empleados = permisosServices.ObtenerEmpleados(),
                MotivosAusencia = permisosServices.ObtenerMotivos(),
                SubEmpresas = permisosServices.ObtenerEmpresas(),
                FechaGeneracion = DateTime.Today
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
        public ActionResult SolicitarPermiso(PermisosVM model)
        {
            if (ModelState.IsValid)
            {
                if (permisosServices.CrearSolicitud(model, out string error))
                {
                    TempData["SolicitudEnviada"] = true;
                    return RedirectToAction("PerfilEmpleado", "Empleados");
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

        #region generar pdf
        public FileResult GenerarPDF(int idEmpresa, string nombre = null, int? departamento = null, int? motivo = null)
        {
            var permisos = permisosServices.ObtenerPermisosPorEmpresaConFiltros(idEmpresa, nombre, departamento, motivo); // Solo aprobados
            var empresa = permisos.FirstOrDefault()?.SubEmpresas?.Empresas;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fuentes
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var subTituloFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var encabezadoFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var celdaFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);

                // Cabecera con logo y datos de empresa
                if (empresa?.Logo != null)
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(empresa.Logo);
                    logo.ScaleAbsolute(70f, 70f);
                    logo.Alignment = Element.ALIGN_LEFT;
                    doc.Add(logo);
                }

                if (empresa != null)
                {
                    doc.Add(new Paragraph(empresa.Nombre, tituloFont));
                    doc.Add(new Paragraph($"RFC: {empresa.Rfc}", subTituloFont));
                    doc.Add(new Paragraph($"Dirección: {empresa.Direccion}", subTituloFont));
                }

                doc.Add(new Paragraph("Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy"), subTituloFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 20f
                });

                // Título del reporte
                Paragraph titulo = new Paragraph("Reporte de Permisos Aprobados", tituloFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(titulo);

                // Tabla
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2.5f, 2.5f, 2.5f, 2.5f, 2f, 1.5f });

                // Encabezados
                BaseColor headerColor = new BaseColor(0, 122, 204);
                string[] headers = { "Empleado", "Motivo", "Fecha Inicio", "Fecha Final", "Generado", "Días" };

                foreach (var h in headers)
                {
                    var cell = new PdfPCell(new Phrase(h, encabezadoFont))
                    {
                        BackgroundColor = headerColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                int totalDias = 0;

                foreach (var p in permisos)
                {
                    int diasLaborales = p.Empleados?.DiasLaboralesPorSemana ?? 5; // Por defecto 5 si es null
                    int diasTomados = ContarDiasHabiles(p.FechaInicio, p.FechaFinal, diasLaborales);
                    totalDias += diasTomados;

                    table.AddCell(new PdfPCell(new Phrase(p.Empleados?.Nombre ?? "N/A", celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.MotivosAusencia?.NombreMotivo ?? "N/A", celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaInicio.ToString("dd/MM/yyyy"), celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaFinal.ToString("dd/MM/yyyy"), celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaGeneracion.ToString("dd/MM/yyyy"), celdaFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(diasTomados.ToString(), celdaFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                // Fila de total
                PdfPCell totalLabel = new PdfPCell(new Phrase("Total días tomados", encabezadoFont))
                {
                    Colspan = 5,
                    BackgroundColor = headerColor,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                table.AddCell(totalLabel);

                PdfPCell totalValue = new PdfPCell(new Phrase(totalDias.ToString(), encabezadoFont))
                {
                    BackgroundColor = headerColor,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(totalValue);

                doc.Add(table);
                doc.Close();
                Response.AppendHeader("Content-Disposition", "inline; filename=Reporte_Permisos_Empresa_" + (empresa?.Nombre ?? "Empresa") + ".pdf");
                return File(ms.ToArray(), "application/pdf");
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
            var data = permisosServices.ObtenerPermisosPorEmpleadosA(id);
            var empleado = data.Item1;
            var permisos = data.Item2;
            var empresa = empleado.SubEmpresas?.Empresas;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fuentes
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var subTituloFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);

                // Tabla encabezado
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 2f, 4f });

                // Logo
                if (empresa?.Logo != null)
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(empresa.Logo);
                    logo.ScaleAbsolute(80f, 80f);
                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        PaddingBottom = 10f
                    };
                    headerTable.AddCell(logoCell);
                }
                else
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                }

                // Datos de la empresa
                PdfPCell empresaInfo = new PdfPCell();
                empresaInfo.Border = Rectangle.NO_BORDER;
                empresaInfo.AddElement(new Paragraph(empresa?.Nombre ?? "Empresa", tituloFont));
                empresaInfo.AddElement(new Paragraph("RFC: " + (empresa?.Rfc ?? "N/A"), subTituloFont));
                empresaInfo.AddElement(new Paragraph("Dirección: " + (empresa?.Direccion ?? "N/A"), subTituloFont));
                empresaInfo.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerTable.AddCell(empresaInfo);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" ")); // Espacio

                // Título
                Paragraph titulo = new Paragraph("Reporte Individual de Permisos", tituloFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                doc.Add(titulo);

                // Datos del empleado
                doc.Add(new Paragraph($"Empleado: {empleado.Nombre}", subTituloFont));
                doc.Add(new Paragraph($"Empresa: {empresa?.Nombre ?? "N/A"}", subTituloFont));
                doc.Add(new Paragraph($"Días de vacaciones otorgados: {empleado.intDiasVacaciones}", subTituloFont));
                doc.Add(new Paragraph($"Días disponibles: {empleado.DiasDisponibles}", subTituloFont));
                doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}", subTituloFont));
                doc.Add(new Paragraph(" ")); // Espacio

                // Tabla permisos
                PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2f, 3f, 2f, 2f, 1.5f });

                // Encabezados
                BaseColor azulEncabezado = new BaseColor(0, 122, 204);
                string[] headers = { "Fecha Generación", "Motivo", "Inicio", "Final", "Días" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = azulEncabezado,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Filas permisos
                int total = 0;
                foreach (var p in permisos)
                {
                    int diasLaborales = empleado.DiasLaboralesPorSemana > 0 ? empleado.DiasLaboralesPorSemana : 5;
                    int totalDias = ContarDiasHabiles(p.FechaInicio, p.FechaFinal, diasLaborales);
                    total += totalDias;

                    table.AddCell(new PdfPCell(new Phrase(p.FechaGeneracion.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.MotivosAusencia?.NombreMotivo ?? "N/A", cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaInicio.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(p.FechaFinal.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(totalDias.ToString(), cellFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                // Total
                PdfPCell totalLabelCell = new PdfPCell(new Phrase("Total días tomados", headerFont))
                {
                    Colspan = 4,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5,
                    BackgroundColor = azulEncabezado
                };
                table.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase(total.ToString(), headerFont))
                {
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = azulEncabezado
                };
                table.AddCell(totalValueCell);

                doc.Add(table);
                doc.Close();

                Response.AppendHeader("Content-Disposition", "inline; filename=ReporteEmpleado_" + empleado.Nombre + ".pdf");
                return File(ms.ToArray(), "application/pdf");

            }
        }




        #endregion

        #region listado de permisos generales 
        [HttpGet]
        public ActionResult Paginacion(int? page, int pageSize = 5)
        { 
            int pageNumber = (page ?? 1); //Pagina por defecto 
            List<Permisos> resultado = permisosServices.PermisosAll()
                .OrderByDescending(p => p.FechaGeneracion).ToList(); //Ordenamiento por fecha 
            //Se aplica paginacon paginacion
            List<PermisosPdfVM> permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            return View(permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }

        #endregion

        public FileResult GenerarPDFPermiso(int idPermiso)
        {
            var permiso = permisosServices.ObtenerPermisoPorId(idPermiso);
            var empleado = permiso.Empleados;

            var empresaObj = permiso.SubEmpresas;
            string empresa = empresaObj?.Nombre ?? "N/A";
            string direccion = empresaObj?.Direccion ?? "Dirección no disponible";
            string rfc = empresaObj?.Rfc ?? "RFC no disponible";
            byte[] logoBytes = empresaObj?.Logo;

            string motivo = permiso.MotivosAusencia?.NombreMotivo ?? "N/A";

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                // Método local para crear una celda con borde
                PdfPCell BorderedCell(Phrase phrase)
                {
                    return new PdfPCell(phrase)
                    {
                        Border = Rectangle.BOX,
                        Padding = 4,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                }

                // Método local para dibujar una copia con borde alrededor
                void AgregarCopia(float yInicio, float altura)
                {
                    PdfContentByte cb = writer.DirectContent;
                    ColumnText ct = new ColumnText(cb);
                    ct.SetSimpleColumn(36, yInicio - altura, PageSize.A4.Width - 36, yInicio);

                    PdfPTable marco = new PdfPTable(1)
                    {
                        WidthPercentage = 100
                    };
                    marco.DefaultCell.Border = Rectangle.BOX;
                    marco.DefaultCell.Padding = 10;

                    PdfPTable contenido = new PdfPTable(1) { WidthPercentage = 100 };

                    // === Encabezado ===
                    PdfPTable headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                    headerTable.SetWidths(new float[] { 0.3f, 0.7f });

                    if (logoBytes != null)
                    {
                        var logo = iTextSharp.text.Image.GetInstance(logoBytes);
                        logo.ScaleToFit(60, 60);
                        headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });
                    }
                    else
                    {
                        headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                    }

                    PdfPCell empresaCell = new PdfPCell { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
                    empresaCell.AddElement(new Paragraph(empresa, boldFont));
                    empresaCell.AddElement(new Paragraph(direccion, normalFont));
                    empresaCell.AddElement(new Paragraph("RFC: " + rfc, normalFont));
                    headerTable.AddCell(empresaCell);

                    contenido.AddCell(new PdfPCell(headerTable) { Border = Rectangle.NO_BORDER, PaddingBottom = 5 });

                    // === Título ===
                    Paragraph titulo = new Paragraph("AVISO DE AUSENCIA", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    contenido.AddCell(new PdfPCell(titulo) { Border = Rectangle.NO_BORDER });

                    // === Datos del permiso con cuadros ===
                    PdfPTable datosTabla = new PdfPTable(2) { WidthPercentage = 100, SpacingBefore = 10, SpacingAfter = 10 };
                    datosTabla.SetWidths(new float[] { 1f, 1f });

                    datosTabla.AddCell(BorderedCell(new Phrase($"No. Folio: {permiso.Id}", normalFont)));
                    datosTabla.AddCell(BorderedCell(new Phrase($"No. Trab: {empleado.NumEmpleado}", normalFont)));

                    datosTabla.AddCell(BorderedCell(new Phrase($"Nombre: {empleado.Nombre}", normalFont)));
                    datosTabla.AddCell(BorderedCell(new Phrase($"Departamento: {empleado.Departamentos?.NombreDep ?? "N/A"}", normalFont)));

                    datosTabla.AddCell(BorderedCell(new Phrase($"Encargado: {empleado.Departamentos?.NombreEmcargado ?? "N/A"}", normalFont)));
                    datosTabla.AddCell(BorderedCell(new Phrase($"Motivo: {motivo}", normalFont)));

                    int diasLaborales = empleado.DiasLaboralesPorSemana > 0 ? empleado.DiasLaboralesPorSemana : 5;
                    int totalDias = ContarDiasHabiles(permiso.FechaInicio, permiso.FechaFinal, diasLaborales);
                    datosTabla.AddCell(BorderedCell(new Phrase($"Días: {totalDias}", normalFont)));
                    datosTabla.AddCell(BorderedCell(new Phrase($"Restan: {permiso.DiasDisponibles} días periodo {DateTime.Now.Year}", normalFont)));

                    datosTabla.AddCell(BorderedCell(new Phrase($"Ausente del: {permiso.FechaInicio:dd-MMMM-yyyy}", normalFont)));
                    datosTabla.AddCell(BorderedCell(new Phrase($"Al: {permiso.FechaFinal:dd-MMMM-yyyy}", normalFont)));

                    contenido.AddCell(new PdfPCell(datosTabla) { Border = Rectangle.NO_BORDER });

                    // === Firmas ===
                    PdfPTable firmas = new PdfPTable(3) { WidthPercentage = 100, SpacingBefore = 20 };
                    firmas.AddCell(CreateSignatureCell("RECURSOS HUMANOS"));
                    firmas.AddCell(CreateSignatureCell("ENC. DEPARTAMENTO"));
                    firmas.AddCell(CreateSignatureCell("INTERESADO"));
                    contenido.AddCell(new PdfPCell(firmas) { Border = Rectangle.NO_BORDER, PaddingTop = 10 });
              
                    // Pie de copia
                    Paragraph footer = new Paragraph($"Documento generado automáticamente - {empresa} | RFC: {rfc}", normalFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 5f
                    };
                    contenido.AddCell(new PdfPCell(footer) { Border = Rectangle.NO_BORDER });

                    marco.AddCell(contenido);
                    ct.AddElement(marco);
                    ct.Go();
                }
         
                // Primera copia: mitad superior
                AgregarCopia(PageSize.A4.Height - 36, (PageSize.A4.Height / 2) - 36);

                // Segunda copia: mitad inferior
                AgregarCopia((PageSize.A4.Height / 2) - 36, (PageSize.A4.Height / 2) - 36);

                doc.Close();
                Response.AppendHeader("Content-Disposition", "inline; filename=AvisoPermiso_" + empleado.Nombre + ".pdf");
                return File(ms.ToArray(), "application/pdf");

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

        private int ContarDiasHabiles(DateTime inicio, DateTime fin, int diasLaboralesPorSemana)
        {
            int diasHabiles = 0;
            DateTime fecha = inicio;

            while (fecha <= fin)
            {
                if (diasLaboralesPorSemana == 5)
                {
                    if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                        diasHabiles++;
                }
                else
                {
                    // Asumimos que si trabaja 6 días, solo descansa domingo
                    if (fecha.DayOfWeek != DayOfWeek.Sunday)
                        diasHabiles++;
                }

                fecha = fecha.AddDays(1);
            }

            return diasHabiles;
        }



        [HttpGet]
        public ActionResult PaginacionPorEmpresa(int idEmpresa, int? departamento, int? motivo, string nombre, int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            var permisos = permisosServices.PermisosPorEmpresa(idEmpresa)
                .Where(p => p.Estado == "Aprobado");

            if (!string.IsNullOrEmpty(nombre))
            {
                permisos = permisos.Where(p => p.Empleados.Nombre.Contains(nombre));
            }

            if (departamento.HasValue && departamento.Value > 0)
            {
                permisos = permisos.Where(p => p.Empleados.IdDepartamento == departamento.Value);
            }

            if (motivo.HasValue && motivo.Value > 0)
            {
                permisos = permisos.Where(p => p.IdTipoPermiso == motivo.Value);
            }

            var resultado = permisos
                .OrderByDescending(p => p.FechaGeneracion)
                .ToList();

            var permisosPdfVM = Mapper.Map<List<PermisosPdfVM>>(resultado);

            ViewBag.EmpresaId = idEmpresa;
            ViewBag.Departamentos = new SelectList(db.Departamentos, "Id", "NombreDep", departamento);
            ViewBag.Motivos = new SelectList(db.MotivosAusencia, "Id", "NombreMotivo", motivo);

            return View("Paginacion", permisosPdfVM.ToPagedList(pageNumber, pageSize));
        }




        [HttpGet]
        public ActionResult CambiarEstado()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CambiarEstado(int id, string estado)
        {
            var permiso = permisosServices.ObtenerPermisoPorId(id); // Debes implementar este método en tu servicio

            if (permisosServices.CambiarEstadoPermiso(id, estado, out string error))
            {
                TempData["EstadoActualizado"] = true;
            }
            else
            {
                TempData["Error"] = error;
            }

            return RedirectToAction("PermisosPendientes", new { idEmpresa = permiso.IdEmpresa });
        }


        [HttpGet]
        public ActionResult PermisosPendientes(int idEmpresa, int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;
            var pendientes = permisosServices.PermisosPorEmpresa(idEmpresa)
                .Where(p => p.Estado == "Pendiente")
                .OrderBy(p => p.FechaGeneracion)
                .ToList();
            var vm = Mapper.Map<List<PermisosPdfVM>>(pendientes);
            ViewBag.EmpresaId = idEmpresa;
            return View("PermisosPendientes", vm.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult PermisosEmpleados(int? idEmpresa, int idEmpleado, int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;
            var permisos = permisosServices.ObtenerPermisosConDetalles()
                .Where(p => p.IdEmpleado == idEmpleado)
                .OrderByDescending(p => p.FechaGeneracion)
                .ToList();
            var vm = Mapper.Map<List<PermisosPdfVM>>(permisos);
            ViewBag.EmpleadoId = idEmpleado;
           
            return View("PermisosEmpleados", vm.ToPagedList(pageNumber, pageSize));
        }
    }
}

