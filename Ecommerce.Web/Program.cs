
using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Services;
using Ecommerce.Infrastructure.Repositories;
using Ecommerce.Infrastructure;
using FluentValidation;
using Ecommerce.Core.Validators;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register services
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
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


// Add other necessary configurations
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
         options.LoginPath = "/Account/Login";
         options.LogoutPath = "/Account/Logout";
         options.AccessDeniedPath = "/Account/AccessDenied";
         options.Cookie.Name = "UserEmail";
    });

var app = builder.Build();

// Add the JWT authentication middleware
app.UseMiddleware<JwtAuthenticationMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Home/Error");
     app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
//using Ecommerce.Core.Interfaces.IFactories;
//using Ecommerce.Core.Interfaces.IRepository;
//using Ecommerce.Core.Interfaces.IServices;
//using Ecommerce.Core.Services;
//using Ecommerce.Infrastructure.Repositories;
//using Ecommerce.Infrastructure;
//using FluentValidation;
//using Ecommerce.Core.Validators;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Register services

//builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
//builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
//builder.Services.AddScoped<IUsersRepository, UserRepository>();
//builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
//builder.Services.AddScoped<ICartsRepository, CartsRepository>();
//builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IProductsService, ProductsService>();
//builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<IOrderService, OrderService>(); 


//// Add other necessary configurations
//builder.Services.AddAuthentication("Cookies")
//    .AddCookie(options =>
//    {
//         options.LoginPath = "/Account/Login";
//         options.LogoutPath = "/Account/Logout";
//         options.AccessDeniedPath = "/Account/AccessDenied";
//         options.Cookie.Name = "UserEmail";
//    });

//var app = builder.Build();

//// Add the JWT authentication middleware
//app.UseMiddleware<JwtAuthenticationMiddleware>();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles(); // Serve static files

//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();

