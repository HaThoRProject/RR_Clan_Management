using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using RR_Clan_Management.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Firestore inicializ�l�sa
try
{
    FirestoreDb db = FirestoreDb.Create("rr-clan-management");
    Console.WriteLine("Firestore kapcsolat sikeresen l�trej�tt!");
}
catch (Exception ex)
{
    Console.WriteLine($"Firestore inicializ�l�si hiba: {ex.Message}");
}

// Firebase inicializ�l�sa
string firebaseCredentialsPath = "/etc/secrets/rr-clan-management.json"; // A Render Secret File el�r�si �tja
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseCredentialsPath);

// Szolg�ltat�sok regisztr�l�sa
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddScoped<FirebaseAuthService>(); // <--- EZ HI�NYZOTT
builder.Services.AddSingleton<LogService>();

builder.Services.AddSingleton(provider =>
{
    string projectId = "rr-clan-management"; // IDE �rd be a projekted azonos�t�j�t
    return FirestoreDb.Create(projectId);
});
builder.Services.AddScoped<IStatService, StatService>();



// Hiteles�t�s be�ll�t�sa
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware konfigur�ci�
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
app.UseAuthentication(); // <--- EZ FONTOS! (K�l�nben a s�tialap� bejelentkez�s nem m�k�dik)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
