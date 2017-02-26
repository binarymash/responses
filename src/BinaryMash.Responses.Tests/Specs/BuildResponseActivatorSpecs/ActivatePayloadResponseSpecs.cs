namespace BinaryMash.Responses.Tests.Specs
{
    using Newtonsoft.Json;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;
    using static BinaryMash.Responses.Tests.Specs.ActivatePayloadResponseSpecs;

    public class ActivatePayloadResponseSpecs : AbstractSpecs<Response<MyTestClass>>
    {
        private Response<MyTestClass> originalResponse;

        private string serializedResponse;

        private Response<MyTestClass> deserializedResponse;

        [Fact]
        public void NoErrors()
        {
            this.When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void AddNullErrors()
        {
            this.Given(_ => GivenWeAddANullError())
                .And(_ => GivenWeAddNullErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void AddEmptyErrors()
        {
            this.Given(_ => GivenWeAddEmptyErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void AddErrors()
        {
            this.Given(_ => GivenWeAddAnError())
                .And(_ => GivenWeAddMoreErrors())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsAllErrors())
                .BDDfy();
        }

        [Fact]
        public void AddPayloadOfIncorrectType()
        {
            this.Given(_ => GivenWeAddAPayloadWithIncorrectType())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenAnExceptionIsThrown())
                .BDDfy();
        }

        [Fact]
        public void AddPayloadOfCorrectType()
        {
            this.Given(_ => GivenWeAddAPayloadWithCorrectType())
                .When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenTheResponseContainsNoErrors())
                .BDDfy();
        }

        [Fact]
        public void DeserializationOfResponseWithAllPropertiesSet()
        {
            this.Given(_ => GivenAResponseWithAllPropertiesSet())
                .When(_ => WhenWeSerializeIt())
                .And(_ => WhenWeDeserializeItAgain())
                .Then(_ => ThenTheDeserializedResponseMatchesTheOriginalResponse())
                .BDDfy();
        }

        [Fact]
        public void DeserializationOfResponseWithNullPayloadAndNoErrors()
        {
            this.Given(_ => GivenAResponseObjectWithNullPayloadAndNoErrors())
                .When(_ => WhenWeSerializeIt())
                .And(_ => WhenWeDeserializeItAgain())
                .Then(_ => ThenTheDeserializedResponseMatchesTheOriginalResponse())
                .BDDfy();
        }

        private void GivenAResponseWithAllPropertiesSet()
        {
            originalResponse = Activator
                .AndWithPayload(new MyTestClass { SomeValue = "ThisIsAValue" })
                .AndWithErrors(new[]
                    {
                        new Error("123", "456"),
                        new Error("789", "012")
                    })
                .Create();
        }

        private void GivenAResponseObjectWithNullPayloadAndNoErrors()
        {
            originalResponse = Activator
                .AndWithPayload(null)
                .Create();
        }

        private void GivenWeAddAPayloadWithIncorrectType()
        {
            Activator.AndWithPayload("boom");
        }

        private void GivenWeAddAPayloadWithCorrectType()
        {
            Activator.AndWithPayload(new MyTestClass() { SomeValue = "abc" });
        }

        private void WhenWeSerializeIt()
        {
            serializedResponse = JsonConvert.SerializeObject(originalResponse);
        }

        private void WhenWeDeserializeItAgain()
        {
            deserializedResponse = JsonConvert.DeserializeObject<Response<MyTestClass>>(serializedResponse);
        }

        private void ThenTheResponseContainsNoErrors()
        {
            Response.Errors.Count.ShouldBe(0);
        }

        private void ThenTheResponseContainsAllErrors()
        {
            Response.Errors.Count.ShouldBe(ExpectedErrors.Count);

            foreach (var expectedError in ExpectedErrors)
            {
                Response.Errors.ShouldContain(e => e.Code == expectedError.Code && e.Message == expectedError.Message);
            }
        }

        private void ThenTheDeserializedResponseMatchesTheOriginalResponse()
        {
            deserializedResponse.Errors.Count.ShouldBe(originalResponse.Errors.Count);

            foreach (var error in originalResponse.Errors)
            {
                deserializedResponse.Errors.ShouldContain(e => e.Code == error.Code && e.Message == error.Message);
            }

            deserializedResponse.Payload?.SomeValue.ShouldBe(originalResponse.Payload?.SomeValue);
        }

        public class MyTestClass
        {
            public string SomeValue { get; set; }
        }
    }
}
