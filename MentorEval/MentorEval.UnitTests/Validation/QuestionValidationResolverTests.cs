using FluentAssertions;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations.Validation;
using System;
using Xunit;

namespace MentorEval.UnitTests.Validation
{
    public class QuestionValidationResolverTests
    {
        [Fact]
        public void Validate_valid_text_does_not_throw()
        {
            var resolver = new QuestionValidationResolver(new IQuestionValidationStrategy[]
            {
            new TextValidationStrategy(),
            new YesNoValidationStrategy(),
            new Scale10ValidationStrategy(),
            new DropdownValidationStrategy()
            });

            var q = new QuestionCreateViewModel { Type = "Text", Text = "OK" };

            Action act = () => resolver.Validate(q);

            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_empty_text_throws()
        {
            var resolver = new QuestionValidationResolver(new IQuestionValidationStrategy[]
            {
            new TextValidationStrategy()
            });

            var q = new QuestionCreateViewModel { Type = "Text", Text = "   " };

            Action act = () => resolver.Validate(q);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Validate_unknown_type_throws()
        {
            var resolver = new QuestionValidationResolver(new IQuestionValidationStrategy[]
            {
            new TextValidationStrategy()
            });

            var q = new QuestionCreateViewModel { Type = "XYZ", Text = "Test" };

            Action act = () => resolver.Validate(q);

            act.Should().Throw<NotSupportedException>();
        }
    }
}
