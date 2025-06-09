using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IPermisosJuarezOServices
    {
        bool CrearPermiso(PermisosJuarezO permisos, out string error);
        EmpleadosJuarezO ObtenerEmpleadoPorId(int id);
        List<EmpleadosJuarezO> ObtenerEmpleados();
        List<SubEmpresasJuarezO> ObtenerEmpresas();
        List<MotivosAusencia> ObtenerMotivos();
        List<PermisosJuarezO> ObtenerPermisosConDetalles();
        Tuple<EmpleadosJuarezO, List<PermisosJuarezO>> ObtenerPermisosPorEmpleados(int idEmpleado);
        List<PermisosJuarezO> PermisosAll();
    }
}