namespace BinaryMash.Responses.Tests.Specs
{
    using System;
    using System.Collections.Generic;
    using Shouldly;

    public abstract class AbstractSpecs<TResponse>
        where TResponse : class
    {
        private Exception thrownException;

        public AbstractSpecs()
        {
            ExpectedErrors = new List<Error>();
            Activator = BuildResponse.ActivatedFrom<TResponse>();
        }

        protected BuildResponseActivator<TResponse> Activator { get; set; }

        protected TResponse Response { get; set; }

        protected List<Error> ExpectedErrors { get; set; }

        protected void GivenWeAddANullError()
        {
            Activator.AndWithErrors((Error)null);
        }

        protected void GivenWeAddNullErrors()
        {
            Activator.AndWithErrors((IEnumerable<Error>)null);
        }

        protected void GivenWeAddEmptyErrors()
        {
            Activator.AndWithErrors(new List<Error>());
        }

        protected void GivenWeAddAnError()
        {
            var error = new Error("1", "A");

            ExpectedErrors.Add(error);
            Activator.AndWithErrors(error);
        }

        protected void GivenWeAddMoreErrors()
        {
            var errors = new List<Error>
            {
                new Error("3", "C"),
                new Error("4", "D")
            };

            ExpectedErrors.AddRange(errors);
            Activator.AndWithErrors(errors);
        }

        protected void WhenWeCreateTheResponse()
        {
            try
            {
                Response = Activator.Create();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
        }

        protected void ThenAnExceptionIsThrown()
        {
            thrownException.ShouldNotBeNull();
        }
    }
}
