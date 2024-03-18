namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Scopes;

#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class ManageNavPages
{
    public static string Painel => "Painel";
    public static string Escopos => "Escopos";
    public static string Grupos => "Grupos";

    public static string PainelNavClass(ViewContext viewContext) => PageNavClass(viewContext, Painel);
    public static string EscoposNavClass(ViewContext viewContext) => PageNavClass(viewContext, Escopos);
    public static string GruposNavClass(ViewContext viewContext) => PageNavClass(viewContext, Grupos);


    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
