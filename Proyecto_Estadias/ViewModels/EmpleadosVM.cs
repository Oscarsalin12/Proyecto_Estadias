
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class EmpleadosVM
    {
        public int Id { get; set; }
        public int NumEmpleado { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaDeIngreso { get; set; }
        public int AniosTrabajados { get; set; }
        public int MesesTrabajados { get; set; }
        public int intDiasVacaciones { get; set; }
        public int IdEmpresa { get; set; }
        public int DiasDisponibles { get; set; }
        public int IdDepartamento { get; set; }

        public List<DepartamentosVM> DepatamentosVM { get; set; } 
        public List<SubEmpresasVM> SubEmpresaVM { get; set; }
        //public virtual Empresas Empresa { get; set; }
    }
}