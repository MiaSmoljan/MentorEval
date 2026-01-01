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

// Kolega servisi za evaluacije
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
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

var app = builder.Build();

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

app.Run();