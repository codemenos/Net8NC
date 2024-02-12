namespace Solucao.Service.API.Core.Configurations;

using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

/// <summary>
/// Configurações do Swagger
/// </summary>
public partial class SwaggerConfiguration
{
    /// <summary>
    /// Configurações dos grupos de APIs
    /// </summary>
    public required Dictionary<string, ApiGroupConfiguration> Groups { get; set; }

    /// <summary>
    /// Caminho do arquivo de comentários XML
    /// </summary>
    public required string XmlCommentsPath { get; set; }
    
    /// <summary>
    /// Outros caminhos de arquivos de comentários XML
    /// </summary>
    public List<string> OtherXmlCommentsPaths { get; set; } = [];

    /// <summary>
    /// Habilita o TryItOut por padrão
    /// </summary>
    public bool EnableTryItOutByDefault { get; set; }

    /// <summary>
    /// Caminho do arquivo JSON customizado
    /// </summary>
    public required string CustomJsonPath { get; set; }

    /// <summary>
    /// Caminho do arquivo JS customizado
    /// </summary>
    public required string CustomJsPath { get; set; }

    /// <summary>
    /// Caminho do arquivo CSS customizado
    /// </summary>
    public required string CustomCssPath { get; set; }

    /// <summary>
    /// Delegados do Swagger
    /// </summary>
    public List<SwaggerDelegate> Delegates { get; set; } = [];

    /// <summary>
    /// Configurações de Grupos de API do Swagger
    /// </summary>
    public class ApiGroupConfiguration
    {
        /// <summary>
        /// Título do grupo
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Versão do grupo
        /// </summary>
        public required string Version { get; set; }
    }

    /// <summary>
    /// Delegados do Swagger
    /// </summary>
    public class SwaggerDelegate
    {
        /// <summary>
        /// Nome do delegado
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Tipo do recurso
        /// </summary>
        public required string ResourceType { get; set; }

        /// <summary>
        /// Tipo de mídia
        /// </summary>
        public required string MediaType { get; set; }

        /// <summary>
        /// Caminho do recurso
        /// </summary>
        public required string Path { get; set; }

        /// <summary>
        /// Recurso
        /// </summary>
        public required string Resource { get; set; }
    }

    /// <summary>
    /// Configurações de Temas do Swagger
    /// </summary>
    public class SwaggerTheme
    {
        /// <summary>
        /// Nome do tema
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Arquivo CSS do tema
        /// </summary>
        public required string File { get; set; }
    }

    /// <summary>
    /// Configurações de Grupos do Swagger
    /// </summary>
    public class SwaggerGroup
    {
        /// <summary>
        /// Nome do grupo
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Valor do grupo
        /// </summary>
        public required string Value { get; set; }
    }

    /// <summary>
    /// Configurações do JSON do Swagger
    /// </summary>
    public class SwaggerJsonConfig
    {
        /// <summary>
        /// Temas do Swagger
        /// </summary>
        public IList<SwaggerTheme> Themes { get; set; } = [];

        /// <summary>
        /// Grupos do Swagger
        /// </summary>
        public IList<SwaggerGroup> Groups { get; set; } = [];

    }
}

/// <summary>
/// Configuração do Swagger
/// </summary>
public partial class SwaggerConfiguration
{
    /// <summary>
    /// Configura o SwaggerGen
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public static void ConfigureSwaggerGen(SwaggerGenOptions options, IConfiguration configuration)
    {
        const string SECTION_CONFIGURATION = "SwaggerConfiguration";
        
        var swaggerConfiguration = configuration.GetSection(SECTION_CONFIGURATION).Get<SwaggerConfiguration>();
        var xmlFile = swaggerConfiguration?.XmlCommentsPath;
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile!);

        foreach (var group in swaggerConfiguration?.Groups!)
        {
            options.SwaggerDoc(group.Key, new OpenApiInfo { Title = group.Value.Title, Version = group.Value.Version });
        }

        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

            var groupName = methodInfo?.DeclaringType?.GetCustomAttributes(true)
                .OfType<ApiExplorerSettingsAttribute>()
                .Select(attr => attr.GroupName)
                .FirstOrDefault();

            return groupName == docName;
        });

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, true);
        }

        foreach (var otherXmlFile in swaggerConfiguration.OtherXmlCommentsPaths)
        {
            var otherXmlPath = Path.Combine(AppContext.BaseDirectory, otherXmlFile);
            if (File.Exists(otherXmlPath))
            {
                options.IncludeXmlComments(otherXmlPath, true);
            }
        }

    }

    /// <summary>
    /// Configura o SwaggerUI
    /// </summary>
    /// <param name="app"></param>
    /// <param name="options"></param>
    public static void ConfigureSwaggerUI(WebApplication app, SwaggerUIOptions options)
    {
        const string SECTION_CONFIGURATION = "SwaggerConfiguration";
        const string MediaTypeJS = "js";
        const string MediaTypeCSS = "css";
        const string SWAGGER_JSON_PATH = "/swagger/{0}/swagger.json";
        const string RESOURCE_STREAM_TYPE_CSS = "text/css";
        const string RESOURCE_STREAM_TYPE_JS = "text/javascript";
        const string THEME = "theme";
        const string SCREEN = "screen";
        const string SWAGGER_CONFIG_JSON_PATH = "/config/swaggerconfig.json";
        const string LINK_CSS = "<link id='csstheme' href='{0}' rel='stylesheet' media='{1}' type='text/css' />";

        var swaggerConfiguration = app.Configuration.GetSection(SECTION_CONFIGURATION).Get<SwaggerConfiguration>();

        foreach (var group in swaggerConfiguration?.Groups!)
        {
            options.SwaggerEndpoint(string.Format(SWAGGER_JSON_PATH, group.Key), group.Key);
        }

        options.EnableTryItOutByDefault();

        var customJsPath = swaggerConfiguration.CustomJsPath;
        var customCssPath = swaggerConfiguration.CustomCssPath;

        if (!string.IsNullOrWhiteSpace(customJsPath))
        {
            options.InjectJavascript(customJsPath);
        }

        if (!string.IsNullOrWhiteSpace(customCssPath))
        {
            var builder = new StringBuilder(options.HeadContent);
            builder.AppendLine(string.Format(LINK_CSS, customCssPath, SCREEN));
            options.HeadContent = builder.ToString();
        }

        app.MapGet(SWAGGER_CONFIG_JSON_PATH, () =>
        {
            var themes = swaggerConfiguration.Delegates
                .Where(d => d.MediaType == MediaTypeCSS && d.ResourceType.Contains(THEME))
                .Select(theme => new SwaggerTheme
                {
                    Name = theme.Name,
                    File = theme.Path
                }).ToList();

            var groups = swaggerConfiguration.Groups.Select(group => new SwaggerGroup
            {
                Name = group.Key,
                Value = string.Format(SWAGGER_JSON_PATH, group.Key)
            }).ToList();

            var jsonConfig = new SwaggerJsonConfig
            {
                Themes = themes,
                Groups = groups
            };

            return Results.Json(jsonConfig);
        });

        foreach (var delegateConfig in swaggerConfiguration.Delegates)
        {
            var path = delegateConfig.Path;
            var resourcePath = delegateConfig.Resource;

            app.MapGet(path, () =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
                var assetFilePath = Path.Combine(assemblyDirectory, "Assets", resourcePath);

                if (File.Exists(assetFilePath))
                {
                    var fileContent = File.ReadAllText(assetFilePath); 

                    if (!string.IsNullOrWhiteSpace(fileContent)) 
                    {
                        switch (delegateConfig.MediaType)
                        {
                            case MediaTypeJS:
                                return Results.Content(fileContent, RESOURCE_STREAM_TYPE_JS);
                            case MediaTypeCSS:
                                return Results.Content(fileContent, RESOURCE_STREAM_TYPE_CSS);
                        }
                    }
                }

                return Results.NotFound();
            })
            .ExcludeFromDescription();
        }
    }
}
