using AutoMapper;
using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace Proyecto_Estadias.Mapping
{
    public class WebProfile:Profile
    {
        public WebProfile()
        {
            CreateMap<Proyecto_Estadias.ViewModels.UsuariosVM, Proyecto_Estadias.Models.Usuarios>()
                .ForMember(dest => dest.Roles, origen => origen.MapFrom(c => c.RolVM))
                .ForMember(dest => dest.Empresas, origen => origen.MapFrom(c => c.empresaVM))
                .ForMember(dest => dest.UsuarioRol, origen => origen.MapFrom(c => c.UsuariosRolesVMs)).ReverseMap();

            CreateMap<Proyecto_Estadias.ViewModels.EmpresaVM, Empresas>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.EmpleadosVM, Empleados>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.UsuariosRolVM, UsuarioRol>().ReverseMap();
        }
            public static void Run()
        {
            AutoMapper.Mapper.Initialize(a =>
            {
                a.AddProfile<WebProfile>();
            });
        }
    }
}