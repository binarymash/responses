using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace BinaryMash.Responses.Tests.Specs
{
    public class ErrorResponseSpecs
    {
        private IEnumerable<Error> _errorsToAdd;

        private Response _response;

        [Fact]
        public void NullInput()
        {
            this.Given(_ => _.GivenThatTheInputErrorCollectionIsNull())
                .When(_ => _.WhenWeCreateTheErrorResponse())
                .Then(_ => _.ThenTheResponseHasNoErrors())
                .BDDfy();
        }

        [Fact]
        public void EmptyCollectionInput()
        {
            this.Given(_ => _.GivenThatTheInputErrorCollectionIsEmpty())
                .When(_ => _.WhenWeCreateTheErrorResponse())
                .Then(_ => _.ThenTheResponseHasNoErrors())
                .BDDfy();
        }

        [Fact]
        public void NonEmptyCollectionInput()
        {
            this.Given(_ => _.GivenThatTheInputErrorCollectionHasMembers())
                .When(_ => _.WhenWeCreateTheErrorResponse())
                .Then(_ => _.ThenTheResponseContainsTheInputErrors())
                .BDDfy();
        }

        private void GivenThatTheInputErrorCollectionIsNull()
        {
            _errorsToAdd = null;
        }

        private void GivenThatTheInputErrorCollectionIsEmpty()
        {
            _errorsToAdd = new List<Error>();
        }

        private void GivenThatTheInputErrorCollectionHasMembers()
        {
            _errorsToAdd = new List<Error>
            {
                new Error(),
                new Error()
            };
        }

        private void WhenWeCreateTheErrorResponse()
        {
            _response = new ErrorResponse(_errorsToAdd);
        }

        private void ThenTheResponseHasNoErrors()
        {
            _response.Errors.Count().ShouldBe(0);
        }

        private void ThenTheResponseContainsTheInputErrors()
        {
            _response.Errors.ShouldBe(_errorsToAdd);
        }

    }
}
