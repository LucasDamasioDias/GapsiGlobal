using GapsiMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;
using GapsiMVC.Data;
using Amazon.S3;
using Amazon;



var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("GapsiMvcContext");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8; 
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddScoped<ConsultaService>();
builder.Services.AddScoped<AtualizarUsuarioService>();
builder.Services.AddScoped<AgendamentoService>();
builder.Services.AddScoped<MensagemService>();

var awsAccessKeyId = builder.Configuration["AWS:AccessKeyId"];
var awsSecretAccessKey = builder.Configuration["AWS:SecretAccessKey"];
var awsRegion = builder.Configuration["AWS:Region"];
var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    return new AmazonS3Client(awsCredentials, RegionEndpoint.GetBySystemName(awsRegion));
});
builder.Services.AddScoped<IS3Service, S3Service>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login"; 
    options.AccessDeniedPath = "/AccessDenied"; 
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<IEmailSender, EmailService>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
