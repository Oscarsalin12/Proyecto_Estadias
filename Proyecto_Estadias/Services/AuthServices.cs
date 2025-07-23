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
    public class AuthServices : IAuthServices // Implementa la interfaz para los metodos 
    {
        // conección a base de datos 
        private readonly Vacaciones_ProyectoEntities db  = null;  // Objeto de base de datos 

        // metodo contructor 
        public AuthServices()
        {
             
            db = new Vacaciones_ProyectoEntities(); // Se instancia el contexto de la base de datos  
        }
       
        #region metodo para inicio de sesion
        // Metodo de inicio de sesión 
        public Usuarios login(string nombreusuario, string contrasenia)
        {
            // Se realiza validación del nombre de usuario y contraseña 
            if (string.IsNullOrWhiteSpace(nombreusuario) || string.IsNullOrWhiteSpace(contrasenia))
            {
                
                return null; // En caso de no tener coinsidencias retorna nulo  
            }
            // Consulta en base de datos trae la primera coinsidencia de nombre y contraseña
            Usuarios usuario = db.Usuarios
                .Include("UsuarioRol") // Incluye los roles del Usuario 
                .FirstOrDefault(u => u.Usuario == nombreusuario && u.Contrasenia == contrasenia);
            
            if (usuario == null) // Validacion de nombre de usuario
            {
                return null; // no existe usuario con permisos 
            }
              
            usuario.UsuarioRol = db.UsuarioRol.Where(u => u.IdUsuario == usuario.Id).ToList(); // Carga los roles asignados con el usuario

            return usuario; // Retorna el usuario con sus roles 
        }

        #endregion

        #region listas auxiliares
        // Consulta para traer Registros de Departamentos ordenados por nombre
        public List<Departamentos> GetDepartamentos()
        {
            return this.db.Departamentos.OrderBy(d => d.NombreDep).ToList();
        }
        // Consulta trae la lista de registros de los roles ordenados por nombre  
        public List<Roles> GetAllRoles()
        {
            return this.db.Roles.OrderBy(p => p.Nombre).ToList();
        }

        // Consulta, trae la lista de registros de las empresa ordenados por nombre 
        public List<Empresas> GetAllEmpresas()
        {
            return this.db.Empresas.OrderBy(p => p.Nombre).ToList();
        }

        // Consulta, trae la lista de registros de las SubEmpresa ordenados por nombre  
        public List<SubEmpresas> ObtenerSubEmpresas()
        {
            return this.db.SubEmpresas.OrderBy(p => p.Nombre).ToList();
        }
    
        #endregion

        #region metodos para crear usuarios 
        // metodo de insercion de usuarios 
        public bool InsertUser(Usuarios usuarios)
        {
         
            bool respuesta = false; // Variale booleana inicalisada en falso

            var transaccion = this.db.Database.BeginTransaction(); // Inicia transaccion para base de datos  

            try    // inicia el intento de insercion
            {
               
                this.db.Usuarios.Add(usuarios); // inserta el usuario resivido 
                
                this.db.SaveChanges();// Guarda los cambios
               
                transaccion.Commit(); // Finaliza la transaccion
               
                return respuesta = true;   // Retorna la respuesta en verdadero
            }
           
            catch (System.Exception ex) // en caso de fallar el intento retorna una excepcion 
            {
               
                transaccion.Rollback(); // reinvierte los cambios en la transaccion 
               
                return respuesta; // retorna la respuesta en falso 
            }
        }

        public bool InsertUser(UsuariosVM usuariosVM, List<int> roles, List<int> empresas)
        {
            
            bool respuesta = false; // Variale booleana inicalisada en falso 


            try // Inicia un intento
            {
                
                Usuarios usuarios = new Usuarios // Crea objeto de usuarios 
                {
                    Usuario = usuariosVM.Usuario.ToLower(), // se asigna el usuario en minusculas, desde el ViewModel, al campo "Usuario" del nuevo objeto
                    Contrasenia = usuariosVM.Contrasenia.ToLower(), // Asigna la contraseña en minusculas, al igual desde el ViewModel, Al campo "contrasenia"
                   
                };

                usuarios.UsuarioRol = new List<UsuarioRol>();

             foreach (var empresaId in empresas) // por cada empresa y rol, asigna una relacion con el UsuarioRol
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

                var transaccion = db.Database.BeginTransaction(); // se inicializa la transaccion 
                db.Usuarios.Add(usuarios); // se añade el nuevo usuario
                db.SaveChanges(); // se guardan cambios en vase de datos
                transaccion.Commit(); // se concluye la transaccion 
                respuesta = true; // retorna verdadero 
            }
            catch (Exception ex) // Entra en la execcion 
            {
                System.Diagnostics.Debug.WriteLine("Error en InsertUser: " + ex.Message);
            }
            
            return respuesta; // retorna falso 
        }

        #endregion

        #region  metodo para perfil 

        public Usuarios GetUsuarioConRelaciones(string nombreusuario)
        {
            return db.Usuarios
                .Include("UsuarioRol.Empresas")
                .Include("UsuarioRol.Roles")
                .FirstOrDefault(u => u.Usuario == nombreusuario); // Trae usuario con roles y empresas 
        }


        public string GetEmpresaNombre(int idEmpresa)
        {
            return db.Empresas.Where(e => e.Id == idEmpresa).Select(e => e.Nombre).FirstOrDefault(); // Trae el nombre de la empresa del usuairo
        }

        #region Listado de SubEmpresas
     
        public string GetSubEmpresasNombre(int idEmpresa_madre)
        {
            return db.SubEmpresas.Where(e => e.IdEmpresa_madre == idEmpresa_madre).Select(e => e.Nombre).FirstOrDefault(); // Trae el nombre de las Sub empresas 
        }
        #endregion 


        #region Listas de empleados por empresas 
     
        public List<Empleados> GetEmpleados(int idEmpresa)
        {
            return db.Empleados
                .Where(e => e.IdEmpresa == idEmpresa)
                .ToList(); // empleados por empresa 
        }

     
        #endregion 

        #endregion

        #region metodos para listado de usuario

        public List<UsuariosVM> UsuariosAll()
        {
            var usuarios = db.Usuarios
                .Include("UsuarioRol.Empresas")
                .Include("UsuarioRol.Roles")
                .ToList(); // Lista completa de los usuarios con rolos y empresas 

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
            }).ToList();  // se transforma a ViewModel
        }
        public bool AsignarRolEmpresa(int idUsuario, int idRol, int idEmpresa)
        {
            try
            {
                // se verifica si ya existe asignacion 
                var existe = db.UsuarioRol.Any(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol && ur.IdEmpresa == idEmpresa);
                if (existe)
                    return false;

                // crea nuevo registro de relacion usuario-rol-empresa
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

            //para empresa 
            if (db.SubEmpresas.Any(s => s.IdEmpresa_madre == idEmpresa)) // se verifica si la empresa tiene subempresas 
            {
                var subempresas = db.SubEmpresas.Where(s => s.IdEmpresa_madre == idEmpresa).Select(s => s.Id).ToList();
                var empleados = db.Empleados.Where(e => subempresas.Contains(e.IdEmpresa)).ToList();

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

        public List<SubEmpresasVM> GetSubEmpresasPorEmpresa(int idEmpresa)
        {
            var subempresas = new List<SubEmpresasVM>();

            subempresas.AddRange(db.SubEmpresas
                .Where(s => s.IdEmpresa_madre == idEmpresa)
                .Select(s => new SubEmpresasVM {
                    Id = s.Id,
                    Nombre = s.Nombre,
                Rfc = s.Rfc,
                Direccion = s.Direccion,
                Logo = s.Logo}));
        

            return subempresas;
        }

        public string GetNombreSubempresas(int idSub)
        {
            return db.SubEmpresas.FirstOrDefault(se => se.Id == idSub)?.Nombre
              
                ?? "Nombre no encontrado";
        }


    }
}


