using Proyecto_Estadias.Filters;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Proyecto_Estadias.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly Vacaciones_ProyectoEntities db = null;
        public AuthServices()
        {
            db = new Vacaciones_ProyectoEntities();
        }
       
        #region metodo para inicio de sesion
        public Usuarios login(string nombreusuario, string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(nombreusuario) || string.IsNullOrWhiteSpace(contrasenia))
            {
                return null;
            }
            Usuarios usuario = db.Usuarios
                .Include("UsuarioRol")
                .FirstOrDefault(u => u.Usuario == nombreusuario && u.Contrasenia == contrasenia);
            if (usuario == null)
            {
                return null;
            }
            usuario.UsuarioRol = db.UsuarioRol.Where(u => u.IdUsuario == usuario.Id).ToList();
            return usuario;
        }

        #endregion

        #region listas auxiliares

        public List<Departamentos> GetDepartamentos()
        {
            return this.db.Departamentos.OrderBy(d => d.NombreDep).ToList();
        }
        public List<Roles> GetAllRoles()
        {
            return this.db.Roles.OrderBy(p => p.Nombre).ToList();
        }

        public List<Empresas> GetAllEmpresas()
        {
            return this.db.Empresas.OrderBy(p => p.Nombre).ToList();
        }

        public List<SubEmpresasMedJ> ObtenerSubEmpresasMed()
        {
            return this.db.SubEmpresasMedJ.OrderBy(p => p.Nombre).ToList();
        }
        public List<SubEmpresasTHH> ObtenerSubEmpresasTHH()
        {
            return this.db.SubEmpresasTHH.OrderBy(p => p.Nombre).ToList();
        }
        public List<SubEmpresasJuarezO> ObtenerSubEmpresasJuarez()
        {
            return this.db.SubEmpresasJuarezO.OrderBy(p => p.Nombre).ToList();
        }
        #endregion

        #region metodos para crear usuarios 

        public bool InsertUser(Usuarios usuarios)
        {
            bool respuesta = false;
            var transaccion = this.db.Database.BeginTransaction();
            try
            {
                this.db.Usuarios.Add(usuarios);
                this.db.SaveChanges();
                transaccion.Commit();
                return respuesta = true;
            }
            catch (System.Exception ex)
            {
                transaccion.Rollback();
                return respuesta;
            }
        }

        public bool InsertUser(UsuariosVM usuariosVM, List<int> roles, List<int> empresas)
        {
            bool respuesta = false;

            try
            {
                Usuarios usuarios = new Usuarios
                {
                    Usuario = usuariosVM.Usuario.ToLower(),
                    Contrasenia = usuariosVM.Contrasenia.ToLower(),
                   
                };

                usuarios.UsuarioRol = new List<UsuarioRol>();

             foreach (var empresaId in empresas)
                {
                    foreach(var rolId in roles)
                    {
                        usuarios.UsuarioRol.Add(new UsuarioRol
                        {
                            IdEmpresa = empresaId,
                            IdRol = rolId,
                            FechaRegistro = DateTime.Now
                        });
                    }
                }

                var transaccion = db.Database.BeginTransaction();
                db.Usuarios.Add(usuarios);
                db.SaveChanges();
                transaccion.Commit();
                respuesta = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en InsertUser: " + ex.Message);
            }

            return respuesta;
        }

        #endregion

        #region  metodo para perfil 

        public Usuarios GetUsuarioConRelaciones(string nombreusuario)
        {
            return db.Usuarios
                .Include("UsuarioRol.Empresas")
                .Include("UsuarioRol.Roles")
                .FirstOrDefault(u => u.Usuario == nombreusuario);
        }


        public string GetEmpresaNombre(int idEmpresa)
        {
            return db.Empresas.Where(e => e.Id == idEmpresa).Select(e => e.Nombre).FirstOrDefault();
        }

        #region Listado de SubEmpresas
        public string GetSubEmpresasNombreJuarez(int idEmpresa_madre)
        {
            return db.SubEmpresasJuarezO.Where(e => e.IdEmpresa_madre == idEmpresa_madre).Select(e => e.Nombre).FirstOrDefault();
        }
        public string GetSubEmpresasNombreMedica(int idEmpresa_madre)
        {
            return db.SubEmpresasMedJ.Where(e => e.IdEmpresa_madre == idEmpresa_madre).Select(e => e.Nombre).FirstOrDefault();
        }
        public string GetSubEmpresasNombreThh(int idEmpresa_madre)
        {
            return db.SubEmpresasTHH.Where(e => e.IdEmpresa_madre == idEmpresa_madre).Select(e => e.Nombre).FirstOrDefault();
        }
        #endregion 


        #region Listas de empleados por empresas 
        public List<EmpleadosJuarezO> GetEmpleadosJuarezOs(int idEmpresa)
        {
            return db.EmpleadosJuarezO
                .Where(e => e.IdEmpresa == idEmpresa)
                .ToList();
        }
        public List<EmpleadosTHH> GetEmpleadosTHH(int idEmpresa)
        {
            return db.EmpleadosTHH
                .Where(e => e.IdEmpresa == idEmpresa)
                .ToList();
        }

        public List<EmpleadosMEDJ> GetEmpleadosMed(int idEmpresa)
        {
            return db.EmpleadosMEDJ
                .Where(e => e.IdEmpresa == idEmpresa)
                .ToList();
        }
        #endregion 

        #endregion

        #region metodos para listado de usuario

        public List<UsuariosVM> UsuariosAll()
        {
            var usuarios = db.Usuarios
                .Include("UsuarioRol.Empresas")
                .Include("UsuarioRol.Roles")
                .ToList();

            return usuarios.Select(u => new UsuariosVM
            {
                Id = u.Id,
                Usuario = u.Usuario,
                Contrasenia = u.Contrasenia,
                UsuariosRolesVMs = u.UsuarioRol.Select(ur => new UsuariosRolVM
                {
                    IdUsuario = ur.IdUsuario,
                    IdRol = ur.IdRol,
                    RolesVM = new RolVM
                    {
                        Id = ur.Roles.Id,
                        Nombre = ur.Roles.Nombre
                    },
                    EmpresaVM = new EmpresaVM
                    {
                        IdEmpresa = ur.Empresas.Id,
                        Nombre = ur.Empresas.Nombre
                    }
                }).ToList()
            }).ToList();
        }
        public bool AsignarRolEmpresa(int idUsuario, int idRol, int idEmpresa)
        {
            try
            {
                var existe = db.UsuarioRol.Any(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol && ur.IdEmpresa == idEmpresa);
                if (existe)
                    return false;

                UsuarioRol nuevo = new UsuarioRol
                {
                    IdUsuario = idUsuario,
                    IdRol = idRol,
                    IdEmpresa = idEmpresa,
                    FechaRegistro = DateTime.Now
                };
                db.UsuarioRol.Add(nuevo);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al asignar rol: " + ex.Message);
                return false;
            }
        }
        #endregion 


        public List<EmpleadosVM> GetEmpleadosPorEmpresaYSubempresa(int idEmpresa)
        {
            var empleadosVM = new List<EmpleadosVM>();

            //para empresa THH 
            if (db.SubEmpresasTHH.Any(s => s.IdEmpresa_madre == idEmpresa))
            {
                var subempresas = db.SubEmpresasTHH.Where(s => s.IdEmpresa_madre == idEmpresa).Select(s => s.Id).ToList();
                var empleados = db.EmpleadosTHH.Where(e => subempresas.Contains(e.IdEmpresa)).ToList();

                empleadosVM.AddRange(empleados.Select(e => new EmpleadosVM
                {
                    Id = e.Id,
                    NumEmpleado = e.NumEmpleado,
                    Nombre = e.Nombre,
                    FechaDeIngreso = e.FechaIDeIngreso,
                    AniosTrabajados = e.AniosTrabajados,
                    MesesTrabajados = e.MesesTrabajados,
                    DiasDisponibles = e.DiasDisponibles,
                    IdEmpresa = e.IdEmpresa,
                    IdDepartamento = e.IdDepartamento

                }));
            }
            else if (db.SubEmpresasMedJ.Any(s => s.IdEmpresa_madre == idEmpresa))
            {
                var subempresas = db.SubEmpresasMedJ.Where(s => s.IdEmpresa_madre == idEmpresa).Select(s => s.Id).ToList();
                var empleados = db.EmpleadosMEDJ.Where(e => subempresas.Contains(e.IdEmpresa)).ToList();

                empleadosVM.AddRange(empleados.Select(e => new EmpleadosVM
                {
                    Id = e.Id,
                    NumEmpleado = e.NumEmpleado,
                    Nombre = e.Nombre,
                    FechaDeIngreso = e.FechaIDeIngreso,
                    AniosTrabajados = e.AniosTrabajados,
                    MesesTrabajados = e.MesesTrabajados,
                    DiasDisponibles = e.DiasDisponibles,
                    IdEmpresa = e.IdEmpresa,
                    IdDepartamento = e.IdDepartamento

                }));
            }
            else if (db.SubEmpresasJuarezO.Any(s => s.IdEmpresa_madre == idEmpresa))
            {
                var subempresas = db.SubEmpresasJuarezO.Where(s => s.IdEmpresa_madre == idEmpresa).Select(s => s.Id).ToList();
                var empleados = db.EmpleadosJuarezO.Where(e => subempresas.Contains(e.IdEmpresa)).ToList();

                empleadosVM.AddRange(empleados.Select(e => new EmpleadosVM
                {
                    Id = e.Id,
                    NumEmpleado = e.NumEmpleado,
                    Nombre = e.Nombre,
                    FechaDeIngreso = e.FechaIDeIngreso,
                    AniosTrabajados = e.AniosTrabajados,
                    MesesTrabajados = e.MesesTrabajados,
                    DiasDisponibles = e.DiasDisponibles,
                    IdEmpresa = e.IdEmpresa,
                    IdDepartamento = e.IdDepartamento

                }));

            }
            return empleadosVM;
        }
    }
}


