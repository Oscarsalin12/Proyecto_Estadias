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
    }
}