using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERPInfinity - YARP API Gateway",
        Version = "v1",
        Description = "Central Reverse Proxy Gateway routing traffic to all ERPInfinity Microservices.",
        Contact = new OpenApiContact
        {
            Name = "ERPInfinity Architecture Team",
            Email = "support@erpinfinity.com"
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERPInfinity YARP API Gateway v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root URL
});

app.MapReverseProxy();

app.Run();
