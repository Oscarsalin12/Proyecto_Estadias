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
            return db.Empleados.Include("SubEmpresas").FirstOrDefault(e => e.Id == id);
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
                DiasDisponibles = empleado.DiasDisponibles - diasSolicitados
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


    }
}