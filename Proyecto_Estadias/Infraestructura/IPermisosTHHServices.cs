using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IPermisosTHHServices
    {
        bool CrearPermiso(PermisosTHH permisos, out string error);
        EmpleadosTHH ObtenerEmpleadoPorId(int id);
        List<EmpleadosTHH> ObtenerEmpleados();
        List<SubEmpresasTHH> ObtenerEmpresas();
        List<MotivosAusencia> ObtenerMotivos();
        List<PermisosTHH> ObtenerPermisosConDetalles();
        Tuple<EmpleadosTHH, List<PermisosTHH>> ObtenerPermisosPorEmpleados(int idEmpleado);
        List<PermisosTHH> PermisosAll();
    }
}