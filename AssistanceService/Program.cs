using AssistanceHandler;
using AssistanceService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);
string modelType = Environment.GetEnvironmentVariable("MODEL_TYPE") ?? "Default";
string GrpcPort = Environment.GetEnvironmentVariable("GRPC_Port") ?? "Default";

Console.WriteLine($"using {modelType} AI");
var port = Int32.Parse(GrpcPort);

if (modelType == "Gemini")
{
    builder.Services.AddScoped<IAssistanceHandler, AssistanceHandler.GoogleAssistanceHandler>();
}
else
{
    builder.Services.AddScoped<IAssistanceHandler, AssistanceHandler.AzureAssistanceHandler>();
}




builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

app.MapGrpcService<AssistanceGrpcService>();
app.MapGet("/", () => "AssistanceService gRPC endpoint.");

app.Run();

