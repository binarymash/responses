namespace BinaryMash.Responses.Tests.Specs
{
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;
    using System.Collections.Generic;

    public class BuildResponseSpecs
    {
        private BuildResponse buildResponse;

        private List<Error> expectedErrors;

        private Response _response;

        [Fact]
        public void NoErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithNoPayload())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void AddNullErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithNoPayload())
                .And(_ => GivenWeAddNullErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void AddEmptyErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithNoPayload())
                .And(_ => GivenWeAddEmptyErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        private void GivenWeWantAResponseWithNoPayload()
        {
            buildResponse = BuildResponse.WithNoPayload();
        }

        private void GivenWeAddNullErrors()
        {
            buildResponse.AndWithErrors(null);
        }

        private void GivenWeAddEmptyErrors()
        {
            buildResponse.AndWithErrors(new List<Error>());
        }

        private void GivenWeAddSomeErrors()
        {
            var errors = new List<Error>
            {
                new Error("1", "A"),
                new Error("2", "B")
            };

            expectedErrors.AddRange(errors);
            buildResponse.AndWithErrors(errors);
        }

        private void GivenWeAddMoreErrors()
        {
            var errors = new List<Error>
            {
                new Error("3", "C"),
                new Error("4", "D")
            };

            expectedErrors.AddRange(errors);
            buildResponse.AndWithErrors(errors);
        }

        private void WhenWeCreateTheResponse()
        {
            _response = buildResponse.Create();
        }

        private void ThenTheResponseContainsNoErrors()
        {
            _response.Errors.Count.ShouldBe(0);
        }

        private void ThenTheResponseContainsAllErrors()
        {
            _response.Errors.Count.ShouldBe(expectedErrors.Count);
            _response.Errors.ShouldBeOneOf(expectedErrors);
        }
    }
}
