using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Estadias.Infraestructura
{
    public interface IEmpleadosJuarezServices
    {
        void ActualizarEmpleado(EmpleadosJuarezO empleado);
        void ActualizarVacacionesYPermisos();
        EmpleadosJuarezO GetEmpleadoPorId(int id);
        void InsertarEmpleados(EmpleadosJuarezO emp);
    }
}
