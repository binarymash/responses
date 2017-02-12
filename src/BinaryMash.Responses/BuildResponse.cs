using System.Collections.Generic;

namespace BinaryMash.Responses
{
    public class BuildResponse
    {
        /// <summary>
        /// Creates a response that does not contain a payload
        /// </summary>
        /// <returns></returns>
        public static BuildResponse WithNoPayload() => new BuildResponse();

        /// <summary>
        /// Creates a response that contains the default value of the specified type
        /// </summary>
        /// <typeparam name="T">The type of payload</typeparam>
        /// <returns></returns>
        public static BuildResponse<T> WithPayload<T>() => new BuildResponse<T>();

        /// <summary>
        /// Creates a response that contains a payload of the specified type
        /// </summary>
        /// <typeparam name="T">The type of payload</typeparam>
        /// <param name="payload">The payload</param>
        /// <returns></returns>
        public static BuildResponse<T> WithPayload<T>(T payload) => new BuildResponse<T>().WithPayload(payload);

        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns></returns>
        public BuildResponse AndWithErrors(Error error)
        {
            if (error != null)
            {
                errors.Add(error);
            }

            return this;
        }

        /// <summary>
        /// Adds a collection of errors to the response
        /// </summary>
        /// <param name="errors">The errors</param>
        /// <returns></returns>
        public BuildResponse AndWithErrors(IEnumerable<Error> errors)
        {
            if (errors != null)
            { 
                this.errors.AddRange(errors);
            }

            return this;
        }

        /// <summary>
        /// Instantiates the response
        /// </summary>
        /// <returns></returns>
        public Response Create() => new Response(errors);

        internal BuildResponse()
        {
            errors = new List<Error>();
        }

        protected List<Error> errors;
    }

    public class BuildResponse<T> 
    {
        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns></returns>
        public BuildResponse<T> AndWithErrors(Error error)
        {
            if (error != null)
            {
                errors.Add(error);
            }

            return this;
        }

        /// <summary>
        /// Adds a collection of errors to the response
        /// </summary>
        /// <param name="errors">The errors</param>
        /// <returns></returns>
        public BuildResponse<T> AndWithErrors(IEnumerable<Error> errors)
        {
            if (errors != null)
            {
                this.errors.AddRange(errors);
            }

            return this;
        }


        /// <summary>
        /// Instantiates the response
        /// </summary>
        /// <returns></returns>
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
