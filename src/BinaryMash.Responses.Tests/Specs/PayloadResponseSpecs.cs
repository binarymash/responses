namespace BinaryMash.Responses.Tests.Specs
{
    using Newtonsoft.Json;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class ResponseSpecscs
    {
        Response<MyTestClass> originalResponse;

        string serializedResponse;

        Response<MyTestClass> deserializedResponse; 

        [Fact]
        public void DeserializationWithJsonDotNet()
        {
            this.Given(_ => GivenAResponseObjectWithErrors())
                .When(_ => WhenWeSerializeIt())
                .And(_ => WhenWeDeserializeItAgain())
                .Then(_ => ThenTheDeserializedResponseMatchesTheOriginalResponse())
                .BDDfy();
        }

        private void GivenAResponseObjectWithErrors()
        {
            originalResponse = BuildResponse
                .WithPayload(new MyTestClass { SomeValue = "ThisIsAValue"})
                .AndWithErrors(new[]
                    {
                        new Error("123", "456"),
                        new Error("789", "012")
                    })
                .Create();
        }

        private void WhenWeSerializeIt()
        {
            serializedResponse = JsonConvert.SerializeObject(originalResponse);
        }

        private void WhenWeDeserializeItAgain()
        {
            deserializedResponse = JsonConvert.DeserializeObject<Response<MyTestClass>>(serializedResponse);
        }

        private void ThenTheDeserializedResponseMatchesTheOriginalResponse()
        {
            deserializedResponse.Errors.Count.ShouldBe(originalResponse.Errors.Count);

            foreach(var error in originalResponse.Errors)
            {
                deserializedResponse.Errors.ShouldContain(e => e.Code == error.Code && e.Message == error.Message);
            }

            deserializedResponse.Payload.SomeValue.ShouldBe(originalResponse.Payload.SomeValue);
        }

        class MyTestClass
        {
            public string SomeValue { get; set; }
        }
    }
}
