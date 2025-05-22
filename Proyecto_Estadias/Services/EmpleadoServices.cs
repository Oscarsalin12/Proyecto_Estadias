using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Services
{
    public class EmpleadoServices : IEmpleadoServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;

        public EmpleadoServices()
        {
            db =new Vacaciones_ProyectoEntities();
        }

        #region metodo para insertar empleados

        public void InsertarEmpleados(Empleados emp)
        {
            var fechaIngreso = emp.FechaIDeIngreso;
            var hoy = DateTime.Now;

            int anios = hoy.Year - fechaIngreso.Year;
            int meses = hoy.Month - fechaIngreso.Month;

            if (hoy.Day < fechaIngreso.Day)
                meses--;

            if (meses < 0)
            {
                anios--;
                meses += 12;
            }

            int diasVacaciones = CalcularDiasVacaciones(anios);

            // Asignar valores calculados al objeto
            emp.AniosTrabajados = anios;
            emp.MesesTrabajados = meses;
            emp.intDiasVacaciones = diasVacaciones;

            // Insertar en la base de datos
            db.Empleados.Add(emp);
            db.SaveChanges();
        }

        private int CalcularDiasVacaciones(int anios)
        {
            if (anios <= 0) return 0;
            if (anios == 1) return 12;
            int dias = 12 + (anios - 1) * 2;
            return Math.Min(dias, 30);
        }
        #endregion
        public void ActualizarEmpleado(Empleados empleado)
        {
            var existente = db.Empleados.Find(empleado.Id);
            if (existente == null) return;

            existente.Nombre = empleado.Nombre;
            existente.NumEmpleado = empleado.NumEmpleado;
            existente.FechaIDeIngreso = empleado.FechaIDeIngreso;
            existente.IdEmpresa = empleado.IdEmpresa;

            // recalcular años, meses y vacaciones
            var hoy = DateTime.Today;
            var fechaIngreso = empleado.FechaIDeIngreso;

            int anios = hoy.Year - fechaIngreso.Year;
            int meses = hoy.Month - fechaIngreso.Month;

            if (hoy.Day < fechaIngreso.Day)
                meses--;

            if (meses < 0)
            {
                anios--;
                meses += 12;
            }

            existente.AniosTrabajados = anios;
            existente.MesesTrabajados = meses;
            existente.intDiasVacaciones = CalcularDiasVacaciones(anios);

            db.SaveChanges();
        }
      
    }
}