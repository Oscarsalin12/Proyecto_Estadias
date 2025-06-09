using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IEmpleadosTHHServices
    {
        void ActualizarEmpleado(EmpleadosTHH empleado);
        void ActualizarVacacionesYPermisos();
        EmpleadosTHH GetEmpleadoPorId(int id);
        void InsertarEmpleados(EmpleadosTHH emp);
    }
}
