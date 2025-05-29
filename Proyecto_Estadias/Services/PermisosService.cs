using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
namespace Proyecto_Estadias.Services
{
    public class PermisosService : IPermisosServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public PermisosService()
        {
            this.db = new Vacaciones_ProyectoEntities();
        }
        public List<Permisos>PermisosAll()
            {
            return db.Permisos.Include("Empresas").Include("Empleados").Include("MotivosAusencia").ToList();
        }

        public List<Empleados> ObtenerEmpleados()
        {
            return db.Empleados.OrderBy(e => e.Nombre).ToList();
        }

        public List<MotivosAusencia> ObtenerMotivos()
        {
            return db.MotivosAusencia.OrderBy(m => m.Id).ToList();
        } 

        public List<Empresas> ObtenerEmpresas()
        {
            return db.Empresas.OrderBy(emp => emp.Id).ToList();
        }
      public Empleados ObtenerEmpleadoPorId(int id)
        {
          return  db.Empleados.Include("Empresas").FirstOrDefault(e => e.Id == id);
        }

        public bool CrearPermiso(Permisos permisos, out string error)
        {
            error = "";
            var empleado = db.Empleados.FirstOrDefault(e => e.Id == permisos.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no emcontrado";
                return false;
            }
            int diasSolicitados = (permisos.FechaFinal - permisos.FechaInicio).Days + 1;
            if (diasSolicitados <= 0 )
            {
                error = "La fecha final no puede ser menor que la fecha de inicio";
                return false;
            }

            if (empleado.DiasDisponibles < diasSolicitados)
            {
                error = "El empleado no tiene dias suficientes ";
                return false;
            }

            empleado.DiasDisponibles -= diasSolicitados;
            db.Permisos.Add(permisos);
            db.SaveChanges();
            return true;
        }

        public List<Permisos> ObtenerPermisosConDetalles()
        {
            return db.Permisos
                .Include("Empresas")
                .Include("Empleados").
                Include("MotivosAusencia").ToList();
        }

        public List<Permisos> ObtenerPermisosPorEmpleados(int id)
        {
            return db.Permisos
                .Include("Empresas").Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == id).ToList();
        }
    }
}