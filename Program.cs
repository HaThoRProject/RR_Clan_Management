using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using RR_Clan_Management.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Firebase inicializálás
string firebaseCredentialsPath = "/etc/secrets/rr-clan-management.json"; // A Render Secret File elérési útja
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseCredentialsPath);

// Szolgáltatások regisztrálása
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddScoped<FirebaseAuthService>(); // <--- EZ HIÁNYZOTT

// Hitelesítés beállítása
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware konfiguráció
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // <--- EZ FONTOS! (Különben a sütialapú bejelentkezés nem mûködik)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
