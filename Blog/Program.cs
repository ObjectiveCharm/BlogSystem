using Blog.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureRepositories();
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureSwagger();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Serve static files and SPA fallback
var clientPath = Path.Combine(builder.Environment.ContentRootPath, "..", "Client", "dist");
if (Directory.Exists(clientPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientPath),
        RequestPath = ""
    });

    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientPath)
    });
}

app.Run();
