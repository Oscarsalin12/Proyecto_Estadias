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

  
        public bool CrearPermiso(Permisos permisos, out string error)
        {
            error = "";

            var empleado = db.Empleados.FirstOrDefault(e => e.Id == permisos.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no encontrado";
                return false;
            }
            
            // Contar solo los días hábiles
            int diasSolicitados = ContarDiasHabiles(permisos.FechaInicio, permisos.FechaFinal);
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

            // Descontar solo días hábiles
            empleado.DiasDisponibles -= diasSolicitados;
            permisos.DiasDisponibles = empleado.DiasDisponibles;
            db.Permisos.Add(permisos);
            db.SaveChanges();

            return true;
        }

        private int ContarDiasHabiles(DateTime inicio, DateTime fin)
        {
            int diasHabiles = 0;
            for (DateTime fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
            {
                if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasHabiles++;
                }
            }
            return diasHabiles;
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