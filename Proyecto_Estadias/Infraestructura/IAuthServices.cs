using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IAuthServices
    {
        List<Empresas> GetAllEmpresas();
        List<Roles> GetAllRoles();
        bool InsertUser(Usuarios usuarios);
        bool InsertUser(UsuariosVM usuariosVM, int Roles, int Empresas);
        Usuarios login(string nombreusuario, string contrasenia);
    }
}