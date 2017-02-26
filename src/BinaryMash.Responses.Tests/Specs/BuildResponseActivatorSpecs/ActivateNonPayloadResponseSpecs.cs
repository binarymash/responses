namespace BinaryMash.Responses.Tests.Specs
{
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class ActivateNonPayloadResponseSpecs : AbstractSpecs<Response>
    {
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
    }
}
