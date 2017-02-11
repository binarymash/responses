using System.Collections.Generic;

namespace BinaryMash.Responses
{
    public class BuildResponse
    {
        public static BuildResponse WithNoPayload() => new BuildResponse();

        public static BuildResponse<T> WithDefaultPayloadOfType<T>() => new BuildResponse<T>();

        public static BuildResponse<T> WithPayload<T>(T payload) => new BuildResponse<T>().WithPayload(payload);

        public BuildResponse AndWithErrors(Error error)
        {
            if (error != null)
            {
                errors.Add(error);
            }

            return this;
        }

        public BuildResponse AndWithErrors(IEnumerable<Error> errors)
        {
            if (errors != null)
            { 
                this.errors.AddRange(errors);
            }

            return this;
        }

        public Response Create() => new Response(errors);

        internal BuildResponse()
        {
            errors = new List<Error>();
        }
        protected List<Error> errors;
    }

    public class BuildResponse<T> 
    {
        public BuildResponse<T> AndWithErrors(Error error)
        {
            if (error != null)
            {
                errors.Add(error);
            }

            return this;
        }

        public BuildResponse<T> AndWithErrors(IEnumerable<Error> errors)
        {
            if (errors != null)
            {
                this.errors.AddRange(errors);
            }

            return this;
        }

        public Response<T> Create() => new Response<T>(payload, errors);

        internal BuildResponse()
        {
            payload = default(T);
            errors = new List<Error>();
        }

        internal BuildResponse<T> WithPayload(T payload)
        {
            this.payload = payload;
            return this;
        }

        protected List<Error> errors;

        private T payload;

    }
}
