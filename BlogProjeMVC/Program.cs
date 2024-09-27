using BlogProjeAPI.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisCache");
    options.InstanceName = builder.Configuration["RedisSettings:InstanceName"];
});

builder.Services.AddTransient<AuthMessageHandler>();

builder.Services.AddHttpClient("BlogClient").AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(int.Parse(builder.Configuration["SessionSettings:IdleTimeoutInMinutes"]));
    options.Cookie.HttpOnly = bool.Parse(builder.Configuration["SessionSettings:CookieHttpOnly"]);
    options.Cookie.IsEssential = bool.Parse(builder.Configuration["SessionSettings:CookieIsEssential"]);
    options.Cookie.Name = builder.Configuration["SessionSettings:CookieName"];
    options.Cookie.HttpOnly = bool.Parse(builder.Configuration["SessionSettings:CookieHttpOnly"]);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;



});

var app = builder.Build();



app.UseExceptionHandler("/");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Use(async (context, next) =>
{
    var cookieName = builder.Configuration["SessionSettings:CookieName"];

    if (!context.Session.Keys.Any() && context.Request.Cookies.ContainsKey(cookieName))
    {
        context.Response.Cookies.Delete(cookieName);
      
    }

    await next.Invoke();


});


app.Run();
