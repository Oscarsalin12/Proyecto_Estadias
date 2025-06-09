using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class PerfilVM
    {
        public Usuarios Usuario { get;set; }
        public EmpleadosJuarezO Empleado { get;set; }
        public EmpleadosMEDJ EmpleadoMedJ { get;set; }
        public EmpleadosTHH EmpleadoTHH { get;set; }
        public List<Empresas> Empresas { get; set; }
        public Dictionary<int, List<EmpleadosVM>> EmpleadosPorEmpresa {  get; set; }
    }
}