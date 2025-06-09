using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IPermisosMedJServices
    {
        bool CrearPermiso(PermisosMedJ permisos, out string error);
        EmpleadosMEDJ ObtenerEmpleadoPorId(int id);
        List<EmpleadosMEDJ> ObtenerEmpleados();
        List<SubEmpresasMedJ> ObtenerEmpresas();
        List<MotivosAusencia> ObtenerMotivos();
        List<PermisosMedJ> ObtenerPermisosConDetalles();
        Tuple<EmpleadosMEDJ, List<PermisosMedJ>> ObtenerPermisosPorEmpleados(int idEmpleado);
        List<PermisosMedJ> PermisosAll();
    }
}