using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Proyecto_Estadias.Helpers
{
    public class ClaimsHelper
    {
        public static int? GetEmpresaId(ClaimsIdentity identity)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == "EmpresaId");
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }

        public static List<int> GetEmpresas(ClaimsIdentity identity)
        {
            return identity.Claims
                .Where(c => c.Type == "EmpresaId")
                .Select(c => int.Parse(c.Value))
                .Distinct()
                .ToList();
        }

        public static List<string> GetRoles(ClaimsIdentity identity)
        {
            return identity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }

        public static bool TieneAccesoARolYEmpresa(ClaimsIdentity identity, string rol, int idEmpresa)
            {
            return identity.Claims.All(c =>
            c.Type == "RolEmpresa" &&
            c.Type == $"{rol} : {idEmpresa}");
        }

    }
}