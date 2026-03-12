using family_tree.Configuration;
using FamilyTree.Extensions;
using FamilyTree.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ISystemConfiguration systemConfiguration = builder.AddSystemConfiguration();

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabase(systemConfiguration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.ApplyDatabaseMigrations();

app.Run();
