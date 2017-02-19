namespace BinaryMash.Responses.Tests.Specs
{
    using System.Collections.Generic;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class BuildResponseSpecs
    {
        private BuildResponse buildResponse;

        private List<Error> expectedErrors;

        private Response response;

        public BuildResponseSpecs()
        {
            expectedErrors = new List<Error>();
        }

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
                .And(_ => GivenWeAddANullError())
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

        [Fact]
        public void AddErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithNoPayload())
                .And(_ => GivenWeAddAnError())
                .And(_ => GivenWeAddMoreErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsAllErrors())
                .BDDfy();
        }

        private void GivenWeWantAResponseWithNoPayload()
        {
            buildResponse = BuildResponse.WithNoPayload();
        }

        private void GivenWeAddANullError()
        {
            buildResponse.AndWithErrors((Error)null);
        }

        private void GivenWeAddNullErrors()
        {
            buildResponse.AndWithErrors((IEnumerable<Error>)null);
        }

        private void GivenWeAddEmptyErrors()
        {
            buildResponse.AndWithErrors(new List<Error>());
        }

        private void GivenWeAddAnError()
        {
            var error = new Error("1", "A");

            expectedErrors.Add(error);
            buildResponse.AndWithErrors(error);
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
            response = buildResponse.Create();
        }

        private void ThenTheResponseContainsNoErrors()
        {
            response.Errors.Count.ShouldBe(0);
        }

        private void ThenTheResponseContainsAllErrors()
        {
            response.Errors.Count.ShouldBe(expectedErrors.Count);

            foreach (var expectedError in expectedErrors)
            {
                response.Errors.ShouldContain(e => e.Code == expectedError.Code && e.Message == expectedError.Message);
            }
        }
    }
}
