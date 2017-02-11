namespace BinaryMash.Responses
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class Response
    {
        internal Response(IList<Error> errors)
        {
            Errors = new ReadOnlyCollection<Error>(errors);
        }

        public IReadOnlyCollection<Error> Errors { get; private set; }
    }

    public class Response<T> : Response
    {
        public T Payload { get; private set; }

        internal Response(T payload, IList<Error> errors) : base(errors)
        {
            Payload = payload;
        }
    }
}
