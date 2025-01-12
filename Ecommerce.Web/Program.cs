using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Services;
using Ecommerce.Core.Validators;
using Ecommerce.Infrastructure.Repositories;
using FluentValidation;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterViewModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginViewModelValidator>();

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
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
