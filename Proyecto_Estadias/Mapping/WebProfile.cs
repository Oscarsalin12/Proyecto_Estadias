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
                //.ForMember(dest => dest.Roles, origen => origen.MapFrom(c => c.RolVM))
                //.ForMember(dest => dest.Empresas, origen => origen.MapFrom(c => c.empresaVM))
                .ForMember(dest => dest.UsuarioRol, origen => origen.MapFrom(c => c.UsuariosRolesVMs)).ReverseMap();

            CreateMap<Proyecto_Estadias.ViewModels.EmpleadosVM, Proyecto_Estadias.Models.EmpleadosJuarezO>()
                .ForMember(dest => dest.SubEmpresasJuarezO, origen => origen.MapFrom(c => c.SubEmpresaVM)).ReverseMap();

            CreateMap<Proyecto_Estadias.ViewModels.EmpleadosVM, Proyecto_Estadias.Models.EmpleadosMEDJ>()
               .ForMember(dest => dest.SubEmpresasMedJ, origen => origen.MapFrom(c => c.SubEmpresaVM)).ReverseMap();

            CreateMap<Proyecto_Estadias.ViewModels.EmpleadosVM, Proyecto_Estadias.Models.EmpleadosTHH>()
                .ForMember(dest => dest.Departamentos, origen => origen.MapFrom(c => c.DepatamentosVM))
               .ForMember(dest => dest.SubEmpresasTHH, origen => origen.MapFrom(c => c.SubEmpresaVM)).ReverseMap();

            CreateMap<Proyecto_Estadias.Models.PermisosJuarezO, Proyecto_Estadias.ViewModels.PermisosVM>()
       .ForMember(dest => dest.EmpleadosJuarezO, opt => opt.Ignore())
       .ForMember(dest => dest.SubEmpresasJuarezOs, opt => opt.Ignore())
       .ForMember(dest => dest.MotivosAusencia, opt => opt.Ignore())
       .ReverseMap(); 
         
        CreateMap<Proyecto_Estadias.Models.PermisosMedJ, Proyecto_Estadias.ViewModels.PermisosVM>()
       .ForMember(dest => dest.EmpleadosMedJ, opt => opt.Ignore())
       .ForMember(dest => dest.SubEmpresasMedJs, opt => opt.Ignore())
       .ForMember(dest => dest.MotivosAusencia, opt => opt.Ignore())
       .ReverseMap();
            
        CreateMap<Proyecto_Estadias.Models.PermisosTHH, Proyecto_Estadias.ViewModels.PermisosVM>()
       .ForMember(dest => dest.EmpleadosTHH, opt => opt.Ignore())
       .ForMember(dest => dest.SubEmpresasTHHs, opt => opt.Ignore())
       .ForMember(dest => dest.MotivosAusencia, opt => opt.Ignore())
       .ReverseMap();

   //         CreateMap<Proyecto_Estadias.Models.Permisos, Proyecto_Estadias.ViewModels.PermisosPdfVM>()
   //.ForMember(dest => dest.Empleados, opt => opt.MapFrom(c => c.Empleados))
   //.ForMember(dest => dest.Empresas, opt => opt.MapFrom(c => c.Empresas))
   //.ForMember(dest => dest.MotivosAusencia, opt => opt.MapFrom(c => c.MotivosAusencia))
   //.ReverseMap();


            CreateMap<Proyecto_Estadias.ViewModels.MotivosAusenciaVM, MotivosAusencia>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.EmpresaVM, Empresas>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.SubEmpresasVM, SubEmpresasJuarezO>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.SubEmpresasVM, SubEmpresasMedJ>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.SubEmpresasVM, SubEmpresasTHH>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.UsuariosRolVM, UsuarioRol>().ReverseMap();
            CreateMap<Proyecto_Estadias.ViewModels.DepartamentosVM, Departamentos>().ReverseMap();
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