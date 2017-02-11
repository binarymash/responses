namespace BinaryMash.Responses.Tests.Specs
{
    using System.Collections.Generic;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class GenericBuildResponseSpecs
    {
        private BuildResponse<SomeTestClass> buildResponse;

        private List<Error> expectedErrors;

        private SomeTestClass expectedPayload;

        private Response<SomeTestClass> response;

        public GenericBuildResponseSpecs()
        {
            expectedErrors = new List<Error>();
        }

        [Fact]
        public void DefaultPayload()
        {
            this.Given(_ => GivenWeWantAResponseWithADefaultPayload())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => TheResponsePayloadHasTheDefaultValue())
                .And(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void DefaultPayloadWithErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithADefaultPayload())
                .And(_ => GivenWeAddAnError())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => TheResponsePayloadHasTheDefaultValue())
                .And(_ => ThenTheResponseContainsAllErrors())
                .BDDfy();
        }


        [Fact]
        public void NullPayload()
        {
            this.Given(_ => GivenWeWantAResponseWithANullPayload())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsTheExpectedPayload())
                .And(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void PayloadWithNoErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithAPayload())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsTheExpectedPayload())
                .And(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void PayloadWithErrors()
        {
            this.Given(_ => GivenWeWantAResponseWithAPayload())
                .And(_ => GivenWeAddAnError())
                .And(_ => GivenWeAddMoreErrors())
                .And(_ => GivenWeAddNullErrors())
                .And(_ => GivenWeAddANullError())
                .And(_ => GivenWeAddEmptyErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsTheExpectedPayload())
                .And(_ => ThenTheResponseContainsAllErrors())
                .BDDfy();
        }

        private void GivenWeWantAResponseWithADefaultPayload()
        {
            buildResponse = BuildResponse.WithDefaultPayloadOfType<SomeTestClass>();
        }

        private void GivenWeWantAResponseWithANullPayload()
        {
            expectedPayload = (SomeTestClass)null;
            buildResponse = BuildResponse.WithPayload<SomeTestClass>(expectedPayload);
        }

        private void GivenWeWantAResponseWithAPayload()
        {
            expectedPayload = new SomeTestClass();
            buildResponse = BuildResponse.WithPayload<SomeTestClass>(expectedPayload);
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

        private void TheResponsePayloadHasTheDefaultValue()
        {
            response.Payload.ShouldBe(default(SomeTestClass));
        }

        private void ThenTheResponseContainsTheExpectedPayload()
        {
            response.Payload.ShouldBe(expectedPayload);
        }

        private void ThenTheResponseContainsNoErrors()
        {
            response.Errors.Count.ShouldBe(0);
        }

        private void ThenTheResponseContainsAllErrors()
        {
            response.Errors.Count.ShouldBe(expectedErrors.Count);

            foreach(var expectedError in expectedErrors)
            {
                response.Errors.ShouldContain(e => e.Code == expectedError.Code && e.Message == expectedError.Message);
            }
        }

        public class SomeTestClass
        {

        }
    }
}
