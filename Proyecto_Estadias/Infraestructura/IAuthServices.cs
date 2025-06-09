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
        bool AsignarRolEmpresa(int idUsuario, int idRol, int idEmpresa);
        List<Empresas> GetAllEmpresas();
        List<Roles> GetAllRoles();
        List<Departamentos> GetDepartamentos();

        #region Lista de empleados
        List<EmpleadosJuarezO> GetEmpleadosJuarezOs(int idEmpresa);
        List<EmpleadosMEDJ> GetEmpleadosMed(int idEmpresa);
        List<EmpleadosVM> GetEmpleadosPorEmpresaYSubempresa(int idEmpresa);
        List<EmpleadosTHH> GetEmpleadosTHH(int idEmpresa);
        #endregion

        #region empresas y sub
        string GetEmpresaNombre(int idEmpresa);
        string GetSubEmpresasNombreJuarez(int idEmpresa_madre);
        string GetSubEmpresasNombreMedica(int idEmpresa_madre);
        string GetSubEmpresasNombreThh(int idEmpresa_madre);
        #endregion

        Usuarios GetUsuarioConRelaciones(string nombreusuario);
        bool InsertUser(Usuarios usuarios);
        //bool InsertUser(UsuariosVM usuariosVM, int Roles, int Empresas);
        bool InsertUser(UsuariosVM usuariosVM, List<int> roles, List<int> empresas);
        Usuarios login(string nombreusuario, string contrasenia);
        List<SubEmpresasJuarezO> ObtenerSubEmpresasJuarez();
        List<SubEmpresasMedJ> ObtenerSubEmpresasMed();
        List<SubEmpresasTHH> ObtenerSubEmpresasTHH();
        List<UsuariosVM> UsuariosAll();
    }
}