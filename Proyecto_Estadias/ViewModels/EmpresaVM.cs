using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class EmpresaVM
    {
        public int Id {  get; set; }
        public string Nombre {  get; set; }
        public RolVM RolesVM { get; set; }
        public UsuariosVM UsuariosVM { get; set; }
        public EmpleadosVM EmpleadosVM { get; set; }
    }
}