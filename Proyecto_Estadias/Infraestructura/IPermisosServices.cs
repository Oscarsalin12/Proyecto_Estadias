using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IPermisosServices
    {
        bool CrearPermiso(Permisos permisos, out string error);
        Empleados ObtenerEmpleadoPorId(int id);
        List<Empleados> ObtenerEmpleados();
        List<MotivosAusencia> ObtenerMotivos();
        List<Permisos> ObtenerPermisosConDetalles();
        List<Permisos> ObtenerPermisosPorEmpleados(int id);
        List<Empresas> ObtenerEmpresas();
        List<Permisos> PermisosAll();
    }
}