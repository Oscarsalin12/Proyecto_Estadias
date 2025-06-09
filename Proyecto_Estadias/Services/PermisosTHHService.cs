using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
namespace Proyecto_Estadias.Services
{
    public class PermisosTHHService : IPermisosTHHServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public PermisosTHHService()
        {
            this.db = new Vacaciones_ProyectoEntities();
        }
        #region Listas auxiliares 
        public List<PermisosTHH> PermisosAll()
        {
            return db.PermisosTHH.Include("SubEmpresasTHH").Include("EmpleadosTHH").Include("MotivosAusencia").ToList();
        }

        public List<EmpleadosTHH> ObtenerEmpleados()
        {
            return db.EmpleadosTHH.OrderBy(e => e.Nombre).ToList();
        }

        public List<MotivosAusencia> ObtenerMotivos()
        {
            return db.MotivosAusencia.OrderBy(m => m.Id).ToList();
        }

        public List<SubEmpresasTHH> ObtenerEmpresas()
        {
            return db.SubEmpresasTHH.OrderBy(emp => emp.Id).ToList();
        }

        #endregion

        #region motodo de permisos
        public EmpleadosTHH ObtenerEmpleadoPorId(int id)
        {
            return db.EmpleadosTHH.Include("SubEmpresasTHH").FirstOrDefault(e => e.Id == id);
        }

        public bool CrearPermiso(PermisosTHH permisos, out string error)
        {
            error = "";
            var empleado = db.EmpleadosTHH.FirstOrDefault(e => e.Id == permisos.IdEmpleado);
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
            db.PermisosTHH.Add(permisos);
            db.SaveChanges();
            return true;
        }

        #endregion

        #region  listado de permisos
        public List<PermisosTHH> ObtenerPermisosConDetalles()
        {
            return db.PermisosTHH
                .Include("SubEmpresasTHH")
                .Include("EmpleadosTHH").
                Include("MotivosAusencia").ToList();
        }

        public Tuple<EmpleadosTHH, List<PermisosTHH>> ObtenerPermisosPorEmpleados(int idEmpleado)
        {
            var empleado = db.EmpleadosTHH.Include("Empresas").FirstOrDefault(e => e.Id == idEmpleado);
            var permisos = db.PermisosTHH
                .Include("SubEmpresasTHH")
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == idEmpleado)
                .ToList();

            return Tuple.Create(empleado, permisos);
        }

        #endregion
    }
}