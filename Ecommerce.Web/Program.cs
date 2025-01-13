using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Services;
using Ecommerce.Core.Validators;
using Ecommerce.Infrastructure.Repositories;
using Ecommerce.Infrastructure.Validators;
using Ecommerce.Infrastructure;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Swagger services
builder.Services.AddControllersWithViews();
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
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IUsersRepository, UserRepository>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICartsRepository, CartsRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
         options.LoginPath = "/Account/Login";
         options.LogoutPath = "/Account/Logout";
         options.AccessDeniedPath = "/Account/AccessDenied";
         options.Cookie.Name = "UserEmail";
    });




var app = builder.Build();

app.UseMiddleware<JwtAuthenticationMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Home/Error");
     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
     app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files

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

app.MapStaticAssets();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );

app.Run();
