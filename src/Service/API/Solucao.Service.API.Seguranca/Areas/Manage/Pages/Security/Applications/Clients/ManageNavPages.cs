namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Clients;

#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class ManageNavPages
{
    public static string Painel => "Painel";
    public static string Clientes => "Clientes";
    public static string Grupos => "Grupos";

    public static string PainelNavClass(ViewContext viewContext) => PageNavClass(viewContext, Painel);
    public static string ClientesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Clientes);
    public static string GruposNavClass(ViewContext viewContext) => PageNavClass(viewContext, Grupos);


    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
