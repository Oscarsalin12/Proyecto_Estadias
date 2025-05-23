using Proyecto_Estadias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Estadias.ViewModels
{
    public class MotivosAusenciaVM
    {
        public int Id { get; set; }
        public string NombreMotivo { get; set; }

        public PermisosVM PermisosVM { get; set; }
    
    }
}