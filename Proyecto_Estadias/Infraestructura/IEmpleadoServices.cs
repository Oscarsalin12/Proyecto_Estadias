using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IEmpleadoServices
    {
        void ActualizarEmpleado(Empleados empleado);
        void ActualizarVacacionesYPermisos();
        Empleados GetEmpleadoPorId(int id);
        void InsertarEmpleados(Empleados emp);
    }
}
