using MentorEval.Models;
using MentorEval.Services;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Evaluations.Questions;
using MentorEval.Services.Evaluations.Validation;
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

builder.Services.AddScoped<IQuestionFactory, QuestionFactory>();

builder.Services.AddScoped<QuestionValidationResolver>();
builder.Services.AddScoped<IQuestionValidationStrategy, Scale10ValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, YesNoValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, TextValidationStrategy>();
builder.Services.AddScoped<IQuestionValidationStrategy, DropdownValidationStrategy>();



// Add services to the container.
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
