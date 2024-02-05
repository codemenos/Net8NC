using Solucao.Service.API.Seguranca.Registers;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.RegisterLogging();

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.RegisterWebApp();

app.Run();
