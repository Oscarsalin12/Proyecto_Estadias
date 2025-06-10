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
        List<SubEmpresas> ObtenerEmpresas();
        List<MotivosAusencia> ObtenerMotivos();
        List<Permisos> ObtenerPermisosConDetalles();
        Tuple<Empleados, List<Permisos>> ObtenerPermisosPorEmpleados(int idEmpleado);
        List<Permisos> PermisosAll();
    }
}