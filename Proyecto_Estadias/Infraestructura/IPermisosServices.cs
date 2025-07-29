using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IPermisosServices
    {
        bool CambiarEstadoPermiso(int idPermiso, string nuevoEstado, out string error);
        bool CrearPermiso(PermisosVM model, out string error);
        bool CrearSolicitud(PermisosVM model, out string error);
        Empleados ObtenerEmpleadoPorId(int id);
        List<Empleados> ObtenerEmpleados();
        List<SubEmpresas> ObtenerEmpresas();
        List<MotivosAusencia> ObtenerMotivos();
        PermisosPdfVM ObtenerPermisoPorId(int idPermiso);
        List<Permisos> ObtenerPermisosConDetalles();
        Tuple<Empleados, List<Permisos>> ObtenerPermisosPorEmpleados(int idEmpleado);
        List<Permisos> PermisosAll();
        List<Permisos> PermisosPorEmpresa(int idEmpresa);
    }
}