using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class PermisosPdfVM
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public int IdTipoPermiso { get; set; }
        public int IdEmpresa { get; set; }
        public DateTime FechaGeneracion { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
        public List<EmpleadosJuarezO> EmpleadosJuarezO { get; set; }
        public List<EmpleadosTHH> EmpleadosTHH { get; set; }
        public List<EmpleadosMEDJ> EmpleadosMedJ { get; set; }
        public Empresas Empresas { get; set; }
        public MotivosAusencia MotivosAusencia { get; set; }
        //public List<Empresas> Empresas { get; set; }
        //public List<Empleados> Empleados { get; set; }
        //public List<MotivosAusencia> MotivosAusencia { get; set; }
    }
}