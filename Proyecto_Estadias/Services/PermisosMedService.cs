using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Services
{
    public class PermisosMedService : IPermisosMedJServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public PermisosMedService()
        {
            this.db = new Vacaciones_ProyectoEntities();
        }
        #region Listas auxiliares 
        public List<PermisosMedJ> PermisosAll()
        {
            return db.PermisosMedJ.Include("SubEmpresasMedJ").Include("EmpleadosMEDJ").Include("MotivosAusencia").ToList();
        }

        public List<EmpleadosMEDJ> ObtenerEmpleados()
        {
            return db.EmpleadosMEDJ.OrderBy(e => e.Nombre).ToList();
        }

        public List<MotivosAusencia> ObtenerMotivos()
        {
            return db.MotivosAusencia.OrderBy(m => m.Id).ToList();
        }

        public List<SubEmpresasMedJ> ObtenerEmpresas()
        {
            return db.SubEmpresasMedJ.OrderBy(emp => emp.Id).ToList();
        }

        #endregion

        #region motodo de permisos
        public EmpleadosMEDJ ObtenerEmpleadoPorId(int id)
        {
            return db.EmpleadosMEDJ.Include("SubEmpresasMedJ").FirstOrDefault(e => e.Id == id);
        }

        public bool CrearPermiso(PermisosMedJ permisos, out string error)
        {
            error = "";
            var empleado = db.EmpleadosMEDJ.FirstOrDefault(e => e.Id == permisos.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no emcontrado";
                return false;
            }
            int diasSolicitados = (permisos.FechaFinal - permisos.FechaInicio).Days + 1;
            if (diasSolicitados <= 0)
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
            db.PermisosMedJ.Add(permisos);
            db.SaveChanges();
            return true;
        }

        #endregion

        #region  listado de permisos
        public List<PermisosMedJ> ObtenerPermisosConDetalles()
        {
            return db.PermisosMedJ
                .Include("SubEmpresasMedJ")
                .Include("EmpleadosMEDJ").
                Include("MotivosAusencia").ToList();
        }

        public Tuple<EmpleadosMEDJ, List<PermisosMedJ>> ObtenerPermisosPorEmpleados(int idEmpleado)
        {
            var empleado = db.EmpleadosMEDJ.Include("SubEmpresasMedJ").FirstOrDefault(e => e.Id == idEmpleado);
            var permisos = db.PermisosMedJ
                .Include("SubEmpresasMedJ")
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == idEmpleado)
                .ToList();

            return Tuple.Create(empleado, permisos);
        }

        #endregion
    }
}