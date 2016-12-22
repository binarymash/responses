namespace BinaryMash.Responses
{
    using System.Collections.Generic;

    public class Response
    {
        private readonly List<Error> _errors = new List<Error>();

        public IEnumerable<Error> Errors => _errors;

        protected void AddErrors(IEnumerable<Error> errors)
        {
            if (errors == null)
            {
                return;
            }

            _errors.AddRange(errors);
        }
    }

    public class Response<T> : Response
    {
        public T Payload { get; private set; }

        public Response(T payload)
        {
            Payload = payload;
        }
    }
}
