using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Services
{
    public class PermisosJuarezOServices : IPermisosJuarezOServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public PermisosJuarezOServices()
        {
            this.db = new Vacaciones_ProyectoEntities();
        }
        #region Listas auxiliares 
        public List<PermisosJuarezO> PermisosAll()
        {
            return db.PermisosJuarezO.Include("SubEmpresasJuarezO").Include("EmpleadosJuarezO").Include("MotivosAusencia").ToList();
        }

        public List<EmpleadosJuarezO> ObtenerEmpleados()
        {
            return db.EmpleadosJuarezO.OrderBy(e => e.Nombre).ToList();
        }

        public List<MotivosAusencia> ObtenerMotivos()
        {
            return db.MotivosAusencia.OrderBy(m => m.Id).ToList();
        }

        public List<SubEmpresasJuarezO> ObtenerEmpresas()
        {
            return db.SubEmpresasJuarezO.OrderBy(emp => emp.Id).ToList();
        }

        #endregion

        #region motodo de permisos
        public EmpleadosJuarezO ObtenerEmpleadoPorId(int id)
        {
            return db.EmpleadosJuarezO.Include("SubEmpresasJuarezO").FirstOrDefault(e => e.Id == id);
        }

        public bool CrearPermiso(PermisosJuarezO permisos, out string error)
        {
            error = "";
            var empleado = db.EmpleadosJuarezO.FirstOrDefault(e => e.Id == permisos.IdEmpleado);
            if (empleado == null)
            {
                error = "Empleado no encontrado";
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
            db.PermisosJuarezO.Add(permisos);
            db.SaveChanges();
            return true;
        }

        #endregion

        #region  listado de permisos
        public List<PermisosJuarezO> ObtenerPermisosConDetalles()
        {
            return db.PermisosJuarezO
                .Include("SubEmpresasJuarezO")
                .Include("EmpleadosJuarezO").
                Include("MotivosAusencia").ToList();
        }

        public Tuple<EmpleadosJuarezO, List<PermisosJuarezO>> ObtenerPermisosPorEmpleados(int idEmpleado)
        {
            var empleado = db.EmpleadosJuarezO.Include("SubEmpresasJuarezO").FirstOrDefault(e => e.Id == idEmpleado);
            var permisos = db.PermisosJuarezO
                .Include("SubEmpresasJuarezO")
                .Include("MotivosAusencia")
                .Where(p => p.IdEmpleado == idEmpleado)
                .ToList();

            return Tuple.Create(empleado, permisos);
        }

        #endregion
    }
}