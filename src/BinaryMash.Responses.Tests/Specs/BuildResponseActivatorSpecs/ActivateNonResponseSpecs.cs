namespace BinaryMash.Responses.Tests.Specs
{
    using TestStack.BDDfy;
    using Xunit;
    using static BinaryMash.Responses.Tests.Specs.ActivateNonResponseSpecs;

    public class ActivateNonResponseSpecs : AbstractSpecs<SomeTestClass>
    {
        [Fact]
        public void CreateFails()
        {
            this.When(_ => WhenWeCreateTheResponse())
                .Then(_ => ThenAnExceptionIsThrown())
                .BDDfy();
        }

        public class SomeTestClass
        {
        }
    }
}
