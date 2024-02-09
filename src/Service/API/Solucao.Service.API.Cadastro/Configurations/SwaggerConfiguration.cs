namespace Solucao.Service.API.Cadastro.Configurations;

using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

public partial class SwaggerConfiguration
{
    public Dictionary<string, ApiGroupConfiguration> Groups { get; set; }
    public string XmlCommentsPath { get; set; }
    public List<string> OtherXmlCommentsPaths { get; set; }
    public bool EnableTryItOutByDefault { get; set; }
    public string CustomJsonPath { get; set; }
    public string CustomJsPath { get; set; }
    public string CustomCssPath { get; set; }
    public List<SwaggerDelegate> Delegates { get; set; }

    public class ApiGroupConfiguration
    {
        public string Title { get; set; }
        public string Version { get; set; }
    }

    public class SwaggerDelegate
    {
        public string Name { get; set; }
        public string ResourceType { get; set; }
        public string MediaType { get; set; }
        public string Path { get; set; }
        public string Resource { get; set; }
    }
    public class SwaggerTheme
    {
        public string Name { get; set; }
        public string File { get; set; }
    }

    public class SwaggerGroup
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class SwaggerJsonConfig
    {
        public IList<SwaggerTheme> Themes { get; set; } = new List<SwaggerTheme>();
        public IList<SwaggerGroup> Groups { get; set; } = new List<SwaggerGroup>();

    }
}
public partial class SwaggerConfiguration
{
    public static void ConfigureSwaggerGen(SwaggerGenOptions options, IConfiguration configuration)
    {
        const string SECTION_CONFIGURATION = "SwaggerConfiguration";
        
        var swaggerConfiguration = configuration.GetSection(SECTION_CONFIGURATION).Get<SwaggerConfiguration>();
        var xmlFile = swaggerConfiguration.XmlCommentsPath;
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        foreach (var group in swaggerConfiguration.Groups)
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

        foreach (var group in swaggerConfiguration.Groups)
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
                var resourceStream = assembly.GetManifestResourceStream(resourcePath);

                if (resourceStream is not null)
                {
                    switch (delegateConfig.MediaType)
                    {
                        case MediaTypeJS:
                            return Results.Stream(resourceStream, RESOURCE_STREAM_TYPE_JS);
                        case MediaTypeCSS:
                            return Results.Stream(resourceStream, RESOURCE_STREAM_TYPE_CSS);
                    }
                }

                return Results.NotFound();
            })
            .ExcludeFromDescription();
        }
    }
}
