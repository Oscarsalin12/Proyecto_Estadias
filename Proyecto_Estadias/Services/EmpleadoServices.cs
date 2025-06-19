using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Services
{
    public class EmpleadoServices : IEmpleadosServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;

        public EmpleadoServices()
        {
            db = new Vacaciones_ProyectoEntities();
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
            emp.DiasDisponibles = diasVacaciones;

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

        #region metodo de actualizacion 
        public void ActualizarEmpleado(Empleados empleado)
        {
            var existe = db.Empleados.Find(empleado.Id);
            if (existe == null) return;

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
            existe.Nombre = empleado.Nombre;
            existe.FechaIDeIngreso = empleado.FechaIDeIngreso;

            if (empleado.IdEmpresa > 0)
            {
                existe.IdEmpresa = empleado.IdEmpresa;

            }
            existe.AniosTrabajados = anios;
            existe.MesesTrabajados = meses;
            existe.intDiasVacaciones = CalcularDiasVacaciones(anios);

            db.SaveChanges();
        }
        #endregion

        #region metodo para calculo de vaciones 

        public void ActualizarVacacionesYPermisos()
        {
            var empleados = db.Empleados.ToList();
            var hoy = DateTime.Today;

            foreach (var emp in empleados)
            {
                var fechaIngreso = emp.FechaIDeIngreso;

                // Cálculo de años y meses trabajados
                int anios = hoy.Year - fechaIngreso.Year;
                int meses = hoy.Month - fechaIngreso.Month;

                if (hoy.Day < fechaIngreso.Day)
                    meses--;

                if (meses < 0)
                {
                    anios--;
                    meses += 12;
                }

                // Días base de vacaciones
                int diasBase = CalcularDiasVacaciones(anios);

                // Sumar solo los días hábiles de cada permiso
                var permisos = db.Permisos.Where(p => p.IdEmpleado == emp.Id).ToList();
                int diasPermisoTomados = permisos.Sum(p => ContarDiasHabiles(p.FechaInicio, p.FechaFinal));

                // Días disponibles
                int diasDisponibles = Math.Max(diasBase - diasPermisoTomados, 0);

                // Guardar
                emp.AniosTrabajados = anios;
                emp.MesesTrabajados = meses;
                emp.intDiasVacaciones = diasBase;
                emp.DiasDisponibles = diasDisponibles;
            }

            db.SaveChanges();
        }

        // Método que cuenta solo días hábiles (lunes a viernes)
        private int ContarDiasHabiles(DateTime inicio, DateTime fin)
        {
            int diasHabiles = 0;
            for (DateTime fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
            {
                if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasHabiles++;
                }
            }
            return diasHabiles;
        }

        public Empleados GetEmpleadoPorId(int id)
        {
            return db.Empleados.FirstOrDefault(e => e.Id == id);
        }

        #endregion
    }
}