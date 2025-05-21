using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class UsuariosVM
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Contrasenia { get; set; }

       
        public int Roles { get; set; }
        public int Empresas { get; set; }
       
        public List<RolVM> RolVM { get; set; }
        public List<EmpresaVM> empresaVM { get; set; }
        public List<UsuariosRolVM> UsuariosRolesVMs { get; set; }
    }
}