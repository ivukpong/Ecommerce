using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Services;
using Ecommerce.Core.Validators;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Ecommerce.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Register Serilog as a singleton
builder.Host.UseSerilog();

// Register the logger explicitly for DI
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services with JWT security definition
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo
     {
          Title = "Ecommerce API",
          Version = "v1",
          Description = "An API to manage an eCommerce platform",
          Contact = new OpenApiContact
          {
               Name = "Iniobong Ukpong",
               Email = "iniobong.ukpong@gtbank.com",
               Url = new Uri("https://iniobongukpong.com"),
          },
     });

     // Add JWT Authentication to Swagger
     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
     {
          In = ParameterLocation.Header,
          Description = "Enter 'Bearer {your JWT token}'",
          Name = "Authorization",
          Type = SecuritySchemeType.Http,
          Scheme = "bearer"
     });

     c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Register services
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IUsersRepository, UserRepository>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICartsRepository, CartsRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

var app = builder.Build();

// Enable Serilog request logging
app.UseSerilogRequestLogging();

// Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Security settings
if (!app.Environment.IsDevelopment())
{
     app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirect root URL ("/") to Swagger UI
app.Use(async (context, next) =>
{
     if (context.Request.Path == "/")
     {
          context.Response.Redirect("/swagger");
          return;
     }
     await next();
});

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API V1");
     c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseEndpoints(endpoints =>
{
     endpoints.MapControllers();
});

app.Run();
