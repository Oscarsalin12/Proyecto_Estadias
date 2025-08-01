using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
namespace Proyecto_Estadias.Services
{
    public class PermisosService : IPermisosServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public PermisosService()
        {
            this.db = new Vacaciones_ProyectoEntities();
        }
        #region Listas auxiliares 
        public List<Permisos> PermisosAll()
        {
            return db.Permisos.Include("SubEmpresas").Include("Empleados").Include("MotivosAusencia").ToList();
        }

        public List<Empleados> ObtenerEmpleados()
        {
            return db.Empleados.OrderBy(e => e.Nombre).ToList();
        }

        public List<MotivosAusencia> ObtenerMotivos()
        {
            return db.MotivosAusencia.OrderBy(m => m.Id).ToList();
        }
    
        public List<SubEmpresas> ObtenerEmpresas()
        {
            return db.SubEmpresas.OrderBy(emp => emp.Id).ToList();
        }

        #endregion

        #region motodo de permisos
        public Empleados ObtenerEmpleadoPorId(int id)
        {
            var permiso = db.Permisos
            .Include("SubEmpresas.Empresas") // Muy importante para cargar Empresa dentro de SubEmpresas
            .Include("Empleados.Departamentos")
            .Include("MotivosAusencia")
            .Where(p => p.Estado == "Aprobado");
            return db.Empleados.FirstOrDefault(e => e.Id == id);

        }


        public Tuple<Empleados, List<Permisos>> ObtenerPermisosPorEmpleadosA(int idEmpleado)
        {
            var empleado = db.Empleados
                .Include("SubEmpresas.Empresas")
                .Include("Departamentos")
                .FirstOrDefault(e => e.Id == idEmpleado);

            var permisosAprobados = db.Permisos
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == idEmpleado && p.Estado == "Aprobado")
                .ToList();

            return Tuple.Create(empleado, permisosAprobados);
        }


        public bool CrearPermiso(PermisosVM model, out string error)
        {
            error = "";

            var empleado = db.Empleados.FirstOrDefault(e => e.Id == model.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no encontrado";
                return false;
            }
            
            // Contar solo los días hábiles
            int diasSolicitados = ContarDiasHabiles(model.FechaInicio, model.FechaFinal,empleado.DiasLaboralesPorSemana);
            if (diasSolicitados <= 0)
            {
                error = "El rango de fechas no contiene días hábiles válidos";
                return false;
            }

            if (empleado.DiasDisponibles < diasSolicitados)
            {
                error = "El empleado no tiene suficientes días disponibles";
                return false;
            }

            var permiso = new Permisos
            {
                IdEmpleado = model.IdEmpleado,
                IdTipoPermiso = model.IdTipoPermiso,
                IdEmpresa = model.IdEmpresa,
                FechaGeneracion = DateTime.Now,
                FechaInicio = model.FechaInicio,
                FechaFinal = model.FechaFinal,
                DiasDisponibles = empleado.DiasDisponibles - diasSolicitados,
                Estado = "Aprobado",
            };

            // Descontar solo días hábiles
            empleado.DiasDisponibles = permiso.DiasDisponibles;
            db.Permisos.Add(permiso);
            db.SaveChanges();

            return true;
        }

        private int ContarDiasHabiles(DateTime fechaInicio, DateTime fechaFin, int diasLaboralesPorSemana)
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


        #endregion

        #region  listado de permisos
        public List<Permisos> ObtenerPermisosConDetalles()
        {
            return db.Permisos
                .Include("SubEmpresas")
                .Include("Empleados").
                Include("MotivosAusencia").ToList();
        }

        public Tuple<Empleados, List<Permisos>> ObtenerPermisosPorEmpleados(int idEmpleado)
        {
            var empleado = db.Empleados.Include("SubEmpresas").FirstOrDefault(e => e.Id == idEmpleado);
            var permisos = db.Permisos
                .Include("SubEmpresas")
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == idEmpleado)
                .ToList();

            return Tuple.Create(empleado, permisos);
        }

        public PermisosPdfVM ObtenerPermisoPorId(int idPermiso)
        {


            var permiso = db.Permisos
   .Include("SubEmpresas.Empresas") // Muy importante para cargar Empresa dentro de SubEmpresas
   .Include("Empleados.Departamentos")
   .Include("MotivosAusencia")
   .FirstOrDefault(p => p.Id == idPermiso);

            if (permiso == null) return null;

            var result = new PermisosPdfVM
            {
                Id = permiso.Id,
                IdEmpleado = permiso.IdEmpleado,
                IdTipoPermiso = permiso.IdTipoPermiso,
                IdEmpresa = permiso.IdEmpresa,
                FechaGeneracion = permiso.FechaGeneracion,
                FechaInicio = permiso.FechaInicio,
                FechaFinal = permiso.FechaFinal,
                Empleados = permiso.Empleados,
                SubEmpresas = permiso.SubEmpresas,
                MotivosAusencia = permiso.MotivosAusencia,
                DiasDisponibles = permiso.DiasDisponibles
                
    };

            return result;
        }
        

        public List<Permisos> PermisosPorEmpresa(int idEmpresa)
        {
            return db.Permisos
                .Include("SubEmpresas")
                .Include("Empleados")
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpresa == idEmpresa)
                .ToList();
        }

        #endregion

        public bool CrearSolicitud(PermisosVM model, out string error)
        {
            error = "";

            var empleado = db.Empleados.FirstOrDefault(e => e.Id == model.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no encontrado";
                return false;
            }

            int diasSolicitados = ContarDiasHabiles(model.FechaInicio, model.FechaFinal, empleado.DiasLaboralesPorSemana);
            if (diasSolicitados <= 0)
            {
                error = "El rango de fechas no contiene días hábiles válidos";
                return false;
            }

            // No descontamos días todavía, hasta que se apruebe
            var permiso = new Permisos
            {
                IdEmpleado = model.IdEmpleado,
                IdTipoPermiso = model.IdTipoPermiso,
                IdEmpresa = model.IdEmpresa,
                FechaGeneracion = DateTime.Now,
                FechaInicio = model.FechaInicio,
                FechaFinal = model.FechaFinal,
                DiasDisponibles = empleado.DiasDisponibles, // todavía sin descontar
                Estado = "Pendiente"
            };

            db.Permisos.Add(permiso);
            db.SaveChanges();
            return true;
        }

        public bool CambiarEstadoPermiso(int idPermiso, string nuevoEstado, out string error)
        {
            error = "";
            var permiso = db.Permisos.Include("Empleados").FirstOrDefault(p => p.Id == idPermiso);
            if (permiso == null)
            {
                error = "Permiso no encontrado";
                return false;
            }

            if (nuevoEstado == "Aprobado")
            {
                int diasSolicitados = ContarDiasHabiles(permiso.FechaInicio, permiso.FechaFinal, permiso.Empleados.DiasLaboralesPorSemana);
                if (permiso.Empleados.DiasDisponibles < diasSolicitados)
                {
                    error = "El empleado no tiene suficientes días disponibles";
                    return false;
                }

                permiso.Empleados.DiasDisponibles -= diasSolicitados;
                permiso.DiasDisponibles = permiso.Empleados.DiasDisponibles;
            }

            permiso.Estado = nuevoEstado;
            db.SaveChanges();
            return true;
        }

        public List<Permisos> ObtenerPermisosPorEmpresa(int idEmpresa)
        {
            return db.Permisos
                .Include("SubEmpresas.Empresas")
                .Include("Empleados")
                .Include("MotivosAusencia")
                .Where(p => p.Estado == "Aprobado" && p.SubEmpresas.IdEmpresa_madre == idEmpresa)
                .ToList();
        }

        public List<Permisos> ObtenerPermisosPorEmpresaConFiltros(int idEmpresa, string nombre, int? departamento, int? motivo)
        {
            var query = db.Permisos
                .Include(p => p.Empleados.Departamentos)
                .Include(p => p.MotivosAusencia)
                .Include(p => p.SubEmpresas.Empresas)
                .Where(p => p.SubEmpresas.IdEmpresa_madre == idEmpresa && p.Estado == "Aprobado");

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(p => p.Empleados.Nombre.Contains(nombre));

            if (departamento.HasValue && departamento != 0)
                query = query.Where(p => p.Empleados.IdDepartamento == departamento.Value);

            if (motivo.HasValue && motivo != 0)
                query = query.Where(p => p.IdTipoPermiso == motivo.Value);

            return query.ToList();
        }



    }
}