using MentorEval.Interfaces;
using MentorEval.Models;
using MentorEval.Services;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Evaluations.Questions;
using MentorEval.Services.Evaluations.Validation;
using MentorEval.Services.TokenStrategies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<PdfService>();

builder.Services.AddScoped<IQuestionFactory, QuestionFactory>();
builder.Services.AddScoped<IQuestionHandler, Scale10QuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, YesNoQuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, TextQuestionHandler>();
builder.Services.AddScoped<IQuestionHandler, DropdownQuestionHandler>();
builder.Services.AddScoped<EvaluationFacade>();
builder.Services.AddScoped<ICourseQueryService, CourseQueryService>();
builder.Services.AddScoped<IEvaluationCreationService, EvaluationCreationService>();
builder.Services.AddScoped<QuestionValidationResolver>();
builder.Services.AddScoped<IQuestionValidationStrategy, Scale10ValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, YesNoValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, TextValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, DropdownValidationStrategy>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserService>(provider =>
{
    var innerService = provider.GetRequiredService<UserService>();
    return new LoggingUserServiceDecorator(innerService);
});

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<VerificationTokenStrategy>();
builder.Services.AddScoped<PasswordResetTokenStrategy>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = ".MentorEval.Auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net; " +
        "style-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self'; " +
        "media-src 'self'; " +
        "object-src 'none'; " +
        "frame-src 'none'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "upgrade-insecure-requests;");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();