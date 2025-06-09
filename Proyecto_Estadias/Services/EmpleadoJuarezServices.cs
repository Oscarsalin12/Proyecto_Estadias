using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.Services
{
    public class EmpleadoJuarezServices : IEmpleadosJuarezServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;

        public EmpleadoJuarezServices()
        {
            db =new Vacaciones_ProyectoEntities();
        }

        #region metodo para insertar empleados

        public void InsertarEmpleados(EmpleadosJuarezO emp)
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
            db.EmpleadosJuarezO.Add(emp);
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
        public void ActualizarEmpleado(EmpleadosJuarezO empleado)
        {
            var existe = db.EmpleadosJuarezO.Find(empleado.Id);
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

            if(empleado.IdEmpresa > 0)
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
            var empleados = db.EmpleadosJuarezO.ToList();
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
                int diasPermisoTomados = db.PermisosJuarezO
                    .Where(p => p.IdEmpleado == emp.Id)
                    .Count();
                int diasDisponibles = Math.Max(diasBase - diasPermisoTomados, 0);

                emp.AniosTrabajados = anios;
                emp.MesesTrabajados = meses;
                emp.intDiasVacaciones = diasBase;
                emp.DiasDisponibles = diasDisponibles;
            }
            db.SaveChanges();
        }
        public EmpleadosJuarezO GetEmpleadoPorId(int id)
        {
            return db.EmpleadosJuarezO.FirstOrDefault(e => e.Id == id);
        }
        #endregion
    }
}