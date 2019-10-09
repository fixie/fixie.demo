namespace ContactList.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation.Results;
    using MediatR;
    using Shouldly;
    using static System.Environment;
    using static Testing;

    public static class Assertions
    {
        public static TException Throws<TException>(this Action action) where TException : Exception
            => Should.Throw<TException>(action);

        public static void ShouldMatch<T>(this IEnumerable<T> actual, params T[] expected)
            => actual.ToArray().ShouldMatch(expected);

        public static void ShouldMatch<T>(this T actual, T expected)
        {
            if (Json(expected) != Json(actual))
                throw new MatchException(expected, actual);
        }

        public static void ShouldValidate<TResult>(this IRequest<TResult> message)
            => Validation(message).ShouldBeSuccessful();

        public static void ShouldNotValidate<TResult>(this IRequest<TResult> message, params string[] expectedErrors)
            => Validation(message).ShouldBeFailure(expectedErrors);

        public static void ShouldBeSuccessful(this ValidationResult result)
        {
            var indentedErrorMessages = result
                .Errors
                .OrderBy(x => x.ErrorMessage)
                .Select(x => "    " + x.ErrorMessage)
                .ToArray();

            var actual = String.Join(NewLine, indentedErrorMessages);

            result.IsValid.ShouldBeTrue($"Expected no validation errors, but found {result.Errors.Count}:{NewLine}{actual}");
        }

        public static void ShouldBeFailure(this ValidationResult result, params string[] expectedErrors)
        {
            result.IsValid.ShouldBeFalse("Expected validation errors, but the message passed validation.");

            result.Errors
                .OrderBy(x => x.ErrorMessage)
                .Select(x => x.ErrorMessage)
                .ShouldMatch(expectedErrors.OrderBy(x => x).ToArray());
        }
    }
}