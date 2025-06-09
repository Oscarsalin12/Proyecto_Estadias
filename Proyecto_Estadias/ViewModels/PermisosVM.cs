using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Estadias.ViewModels
{
    public class PermisosVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Selecciones un empleado")]
        public int IdEmpleado { get; set; }
        [Required(ErrorMessage = "Seleccione el motivo")]
        public int IdTipoPermiso { get; set; }
        public int IdEmpresa { get; set; }
        public DateTime FechaGeneracion { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaFinal { get; set; }

        public List<Empresas> Empresas { get; set; }
        public List<EmpleadosJuarezO> EmpleadosJuarezO { get; set; }
        public List<EmpleadosTHH> EmpleadosTHH { get; set; }
        public List<EmpleadosMEDJ> EmpleadosMedJ { get; set; }
        public List<SubEmpresasJuarezO> SubEmpresasJuarezOs { get; set; }
        public List<SubEmpresasMedJ> SubEmpresasMedJs { get; set; }
        public List<SubEmpresasTHH> SubEmpresasTHHs { get; set; }


        public List<MotivosAusencia> MotivosAusencia { get; set; }
    }
}