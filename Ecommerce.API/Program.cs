using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Services;
using Ecommerce.Core.Validators;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services
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
});

// Register other necessary services
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Home/Error");
     app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enable middleware to serve generated Swagger as a JSON endpoint
app.UseSwagger();

// Enable middleware to serve Swagger UI (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API V1");
     c.RoutePrefix = "swagger";
});

app.UseEndpoints(endpoints =>
{
     endpoints.MapControllers();
});

app.Run();
