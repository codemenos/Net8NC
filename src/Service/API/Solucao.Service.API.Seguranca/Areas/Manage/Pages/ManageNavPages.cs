namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages;

#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class ManageNavPages
{
    public static string Painel => "Painel";

    public static string Aplicacao => "Aplicações";

    public static string Identidade => "Identidade";

    public static string PainelNavClass(ViewContext viewContext) => PageNavClass(viewContext, Painel);

    public static string AplicacaoNavClass(ViewContext viewContext) => PageNavClass(viewContext, Aplicacao);

    public static string IdentidadeNavClass(ViewContext viewContext) => PageNavClass(viewContext, Identidade);



    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
