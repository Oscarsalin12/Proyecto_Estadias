using Proyecto_Estadias.Filters;
using Proyecto_Estadias.Infraestructura;
using Proyecto_Estadias.Models;
using Proyecto_Estadias.ViewModels;
using System;
using System.Collections.Generic;
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
        public List<Roles> GetAllRoles()
        {
            return this.db.Roles.OrderBy(p => p.Nombre).ToList();
        }

        public List<Empresas> GetAllEmpresas()
        {
            return this.db.Empresas.OrderBy(p => p.Nombre).ToList();
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

        public bool InsertUser(UsuariosVM usuariosVM, int Roles, int Empresas)
        {
            bool respuesta = false;

            try
            {
                Usuarios usuarios = new Usuarios
                {
                    Usuario = usuariosVM.Usuario.ToLower(),
                    Contrasenia = usuariosVM.Contrasenia.ToLower(),
                    IdEmpresa = Empresas,
                    IdRol = Roles
                };

                List<UsuarioRol> usuariosroles = new List<UsuarioRol>();
                usuariosroles.Add(new UsuarioRol
                {
                    IdRol = Roles,
                    IdEmpresa = Empresas,
                    FechaRegistro = DateTime.Now,
                    Usuarios = usuarios
                });

                usuarios.UsuarioRol = usuariosroles;

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
                .Include("Empresas")
                .Include("Roles")
                .FirstOrDefault(u => u.Usuario == nombreusuario);
        }

        public List<Empleados> GetEmpleadosPorEmpresa(int idEmpresa)
        {
            return db.Empleados
                .Where(e => e.IdEmpresa == idEmpresa)
                .ToList();
        }
        public string GetEmpresaNombre(int idEmpresa)
        {
            return db.Empresas.Where(e => e.Id == idEmpresa).Select(e => e.Nombre).FirstOrDefault();
        }

        #endregion 
    }
}
