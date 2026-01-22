using MentorEval.Models;
using MentorEval.Services;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Evaluations.Questions;
using MentorEval.Services.Evaluations.Validation;
using MentorEval.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// ----------------------------
// App services
// ----------------------------
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ReportAnalyticsService>();

// Clean architecture / improvements (from Ivan's version)
builder.Services.AddScoped<IQuestionFactory, QuestionFactory>();
builder.Services.AddScoped<IQuestionHandler, Scale10QuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, YesNoQuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, TextQuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, DropdownQuestionHandler>();

builder.Services.AddScoped<EvaluationFacade>();

builder.Services.AddScoped<ICourseQueryService, CourseQueryService>();
builder.Services.AddScoped<IEvaluationCreationService, EvaluationCreationService>();
builder.Services.AddScoped<IEvaluationQueryService, EvaluationQueryService>();

builder.Services.AddScoped<QuestionValidationResolver>();
builder.Services.AddScoped<IQuestionValidationStrategy, Scale10ValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, YesNoValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, TextValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, DropdownValidationStrategy>();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// MVC
builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// Basic security headers (no functional impact)
app.Use(async (context, next) =>
{
    if (context.Request.IsHttps)
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "style-src 'self' https://cdn.jsdelivr.net; " +
        "script-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net;";

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // still keep consistent behavior
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
