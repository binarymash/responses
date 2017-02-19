namespace BinaryMash.Responses
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class Response
    {
        public Response(IList<Error> errors)
        {
            Errors = new ReadOnlyCollection<Error>(errors);
        }

        public IReadOnlyCollection<Error> Errors { get; private set; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class Response<T> : Response
#pragma warning restore SA1402 // File may only contain a single class
    {
        public Response(T payload, IList<Error> errors)
            : base(errors)
        {
            Payload = payload;
        }

        public T Payload { get; private set; }
    }
}
