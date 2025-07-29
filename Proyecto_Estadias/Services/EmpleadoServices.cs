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
        public Empleados ObtenerEmpleadoPorId(int id)
        {
            return db.Empleados
                .Include("Departamentos")
                .Include("SubEmpresas")
                .ToList()
                .FirstOrDefault(e => e.Id == id);
        }



        public void ActualizarEmpleado(Empleados empleadoActualizado)
        {
            var empleado = db.Empleados.Find(empleadoActualizado.Id);
            if (empleado != null)
            {
                empleado.NumEmpleado = empleadoActualizado.NumEmpleado;
                empleado.Nombre = empleadoActualizado.Nombre;
                empleado.FechaIDeIngreso = empleadoActualizado.FechaIDeIngreso;
                empleado.IdEmpresa = empleadoActualizado.IdEmpresa;
                empleado.IdDepartamento = empleadoActualizado.IdDepartamento;
                empleado.DiasLaboralesPorSemana = empleadoActualizado.DiasLaboralesPorSemana;

                db.SaveChanges();
            }
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

                int anios = hoy.Year - fechaIngreso.Year;
                int meses = hoy.Month - fechaIngreso.Month;

                if (hoy.Day < fechaIngreso.Day)
                    meses--;

                if (meses < 0)
                {
                    anios--;
                    meses += 12;
                }

                int diasBase = CalcularDiasVacaciones(anios);

                int diasLaborales = (emp.DiasLaboralesPorSemana >= 5 && emp.DiasLaboralesPorSemana <= 6)
                                    ? emp.DiasLaboralesPorSemana : 5;

                var permisos = db.Permisos.Where(p => p.IdEmpleado == emp.Id && p.Estado ==  "Aprobado").ToList();

                int diasPermisoTomados = permisos.Sum(p =>
                    ContarDiasHabiles(p.FechaInicio, p.FechaFinal, diasLaborales)
                );

                int diasDisponibles = Math.Max(diasBase - diasPermisoTomados, 0);

                emp.AniosTrabajados = anios;
                emp.MesesTrabajados = meses;
                emp.intDiasVacaciones = diasBase;
                emp.DiasDisponibles = diasDisponibles;
            }

            db.SaveChanges();
        }



        // Método que cuenta solo días hábiles (lunes a viernes)
        private int ContarDiasHabiles(DateTime fechaInicio, DateTime fechaFin, int diasLaboralesPorSemana = 5)
        {
            int total = 0;
            for (var date = fechaInicio.Date; date <= fechaFin.Date; date = date.AddDays(1))
            {
                if (diasLaboralesPorSemana == 6)
                {
                    if (date.DayOfWeek != DayOfWeek.Sunday)
                        total++;
                }
                else
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                        total++;
                }
            }
            return total;
        }

        public Empleados GetEmpleadoPorId(int id)
        {
            return db.Empleados.FirstOrDefault(e => e.Id == id);
        }

        #endregion
        
       public Empleados LoginEmpleados(string usuario, int numeroEmpleado)
        {
          
            if (string.IsNullOrWhiteSpace(usuario))
            return null; // O lanzar una excepción si prefieres
            var empleado = db.Empleados.FirstOrDefault(e => e.UsuarioEmpleado == usuario && e.NumEmpleado == numeroEmpleado);
                return empleado;
          
        }

    }
}