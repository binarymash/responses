namespace BinaryMash.Responses.Tests.Specs
{
    using System;
    using System.Linq;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class ResponseSpecs
    {

        private Response _response;

        private SomeTestClass _inputPayload;

        private Response<SomeTestClass> _responseWithPayload;

        [Fact]
        public void EmptyResponse()
        {
            this.When(_ => _.WhenCreatingAResponseWithDefaultConstructor())
                .Then(_ => _.ThenTheResponseHasNoErrors())
                .BDDfy();
        }

        [Fact]
        public void ResponseWithNullPayload()
        {
            this.Given(_ => _.GivenANullPayload())
                .When(_ => _.WhenCreatingAResponseWithPayloadConstructor())
                .Then(_ => _.ThenTheResponseContainsThePayload())
                .Then(_ => _.ThenThePayloadResponseHasNoErrors())
                .BDDfy();
        }

        [Fact]
        public void ResponseWithPayload()
        {
            this.Given(_ => _.GivenAPayload())
                .When(_ => _.WhenCreatingAResponseWithPayloadConstructor())
                .Then(_ => _.ThenTheResponseContainsThePayload())
                .Then(_ => _.ThenThePayloadResponseHasNoErrors())
                .BDDfy();
        }

        private void GivenANullPayload()
        {
            _inputPayload = null;
        }

        private void GivenAPayload()
        {
            _inputPayload = new SomeTestClass();
        }

        private void WhenCreatingAResponseWithDefaultConstructor()
        {
            _response = new Response();
        }

        private void WhenCreatingAResponseWithPayloadConstructor()
        {
            _responseWithPayload = new Response<SomeTestClass>(_inputPayload);
        }

        private void ThenTheResponseContainsThePayload()
        {
            _responseWithPayload.Payload.ShouldBe(_inputPayload);
        }

        private void ThenTheResponseHasNoErrors()
        {
            _response.Errors.Count().ShouldBe(0);
        }

        private void ThenThePayloadResponseHasNoErrors()
        {
            _responseWithPayload.Errors.Count().ShouldBe(0);
        }

        public class SomeTestClass
        {
            
        }
    }
}
