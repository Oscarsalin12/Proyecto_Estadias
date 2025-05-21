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
        List<Empleados> GetEmpleadosPorEmpresa(int idEmpresa);
        string GetEmpresaNombre(int idEmpresa);
        Usuarios GetUsuarioConRelaciones(string nombreusuario);
        bool InsertUser(Usuarios usuarios);
        bool InsertUser(UsuariosVM usuariosVM, int Roles, int Empresas);
        Usuarios login(string nombreusuario, string contrasenia);
    }
}