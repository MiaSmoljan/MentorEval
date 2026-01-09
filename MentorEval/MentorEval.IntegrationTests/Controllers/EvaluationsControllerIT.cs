using FluentAssertions;
using MentorEval.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace MentorEval.IntegrationTests.Controllers
{
    public class EvaluationsControllerIT : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EvaluationsControllerIT(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true 
            });
        }

        [Fact]
        public async Task Get_Create_returns_200()
        {
            var res = await _client.GetAsync("/Evaluations/Create");

            res.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var html = await res.Content.ReadAsStringAsync();
            html.Should().Contain("Create evaluation");
        }

        [Fact]
        public async Task Post_Create_valid_model_redirects()
        {
            var getRes = await _client.GetAsync("/Evaluations/Create");
            getRes.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var html = await getRes.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,

                ["CourseId"] = "10",
                ["Title"] = "Eval Web",
                ["StartAt"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["EndAt"] = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),

                ["Questions[0].Text"] = "Komentar?",
                ["Questions[0].Type"] = "Text",
                ["Questions[0].Required"] = "true",

                ["Questions[1].Text"] = "Preporučuješ?",
                ["Questions[1].Type"] = "YesNo",
                ["Questions[1].Required"] = "false"
            };

            var postRes = await _client.PostAsync("/Evaluations/Create", new FormUrlEncodedContent(form));

            var body = await postRes.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            ((int)postRes.StatusCode).Should().BeInRange(300, 399);
            postRes.Headers.Location!.ToString().Should().Contain("/Evaluations");
        }

        private static string ExtractAntiForgeryToken(string html)
        {
            var match = Regex.Match(
                html,
                @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
                RegexOptions.IgnoreCase);

            match.Success.Should().BeTrue("antiforgery token input should exist in the form");
            return match.Groups[1].Value;
        }

        [Fact]
        public async Task Index_contains_expected_ui_elements()
        {
            var res = await _client.GetAsync("/Evaluations");
            var html = await res.Content.ReadAsStringAsync();

            html.Should().Contain("Available evaluations");
            html.Should().Contain("Create evaluation!");
        }

        [Fact]
        public async Task Create_view_contains_questions_and_add_button()
        {
            var res = await _client.GetAsync("/Evaluations/Create");
            var html = await res.Content.ReadAsStringAsync();

            html.Should().Contain("Questions");
            html.Should().Contain("id=\"add-question\"");
        }

    }
}
