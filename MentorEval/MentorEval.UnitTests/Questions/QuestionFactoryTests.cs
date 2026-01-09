using FluentAssertions;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Evaluations.Questions;
using System;
using Xunit;

namespace MentorEval.UnitTests.Questions
{
    public class QuestionFactoryTests
    {
        [Fact]
        public void Create_Text_question_maps_fields()
        {
            var factory = new QuestionFactory(new IQuestionHandler[]
            {
            new TextQuestionHandler(),
            new YesNoQuestionHandler(),
            new Scale10QuestionHandler(),
            new DropdownQuestionHandler()
            });

            var vm = new QuestionCreateViewModel
            {
                Type = "Text",
                Text = "Komentar?",
                Required = true
            };

            var q = factory.Create(vm);

            q.Type.Should().Be("Text");
            q.Text.Should().Be("Komentar?");
            q.Required.Should().BeTrue();
        }

        [Fact]
        public void Create_unknown_type_throws()
        {
            var factory = new QuestionFactory(new IQuestionHandler[]
            {
            new TextQuestionHandler()
            });

            var vm = new QuestionCreateViewModel { Type = "XYZ", Text = "Test" };

            Action act = () => factory.Create(vm);

            act.Should().Throw<NotSupportedException>();
        }
    }
}
