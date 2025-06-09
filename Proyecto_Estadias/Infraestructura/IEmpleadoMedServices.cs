using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IEmpleadoMedServices
    {
        void ActualizarEmpleado(EmpleadosMEDJ empleado);
        void ActualizarVacacionesYPermisos();
        EmpleadosMEDJ GetEmpleadoPorId(int id);
        void InsertarEmpleados(EmpleadosMEDJ emp);
    }
}
