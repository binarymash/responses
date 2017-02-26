namespace BinaryMash.Responses
{
    using System.Collections.Generic;

    public class BuildResponse
    {
        private List<Error> errors;

        internal BuildResponse()
        {
            errors = new List<Error>();
        }

        /// <summary>
        /// Creates a response that does not contain a payload
        /// </summary>
        /// <returns>The BuildResponse pipeline</returns>
        public static BuildResponse WithNoPayload() => new BuildResponse();

        /// <summary>
        /// Creates a response that contains the default value of the specified type
        /// </summary>
        /// <typeparam name="TPayload">The type of payload</typeparam>
        /// <returns>The payload-aware BuildResponse pipeline</returns>
        public static BuildResponse<TPayload> WithPayload<TPayload>() => new BuildResponse<TPayload>();

        /// <summary>
        /// Creates a response of the specified type. This is intended to be used when
        /// the exact type of response is not known at compile time. Try to use WithNoPayload()
        /// or WithPayload&lt;T&gt; in preference to this method, as they provide stronger type safety.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to create.</typeparam>
        /// <returns>The BuildsResponseActivator pipeline</returns>
        public static BuildResponseActivator<TResponse> WithType<TResponse>()
            where TResponse : class
        {
            return new BuildResponseActivator<TResponse>();
        }

        /// <summary>
        /// Creates a response that contains a payload of the specified type
        /// </summary>
        /// <typeparam name="TPayload">The type of payload</typeparam>
        /// <param name="payload">The payload</param>
        /// <returns>The BuildResponse pipeline</returns>
        public static BuildResponse<TPayload> WithPayload<TPayload>(TPayload payload) => new BuildResponse<TPayload>().WithPayload(payload);

        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns>The BuildResponse pipeline</returns>
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
        /// <returns>The BuildResponse pipeline</returns>
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
        /// <returns>The Response instance</returns>
        public Response Create() => new Response(errors);
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class BuildResponse<TPayload>
#pragma warning restore SA1402 // File may only contain a single class
    {
        private List<Error> errors;

        private TPayload payload;

        internal BuildResponse()
        {
            payload = default(TPayload);
            errors = new List<Error>();
        }

        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns>The payload-aware BuildResponse pipeline</returns>
        public BuildResponse<TPayload> AndWithErrors(Error error)
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
        /// <returns>The payload-aware BuildResponse pipeline</returns>
        public BuildResponse<TPayload> AndWithErrors(IEnumerable<Error> errors)
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
        /// <returns>The Response instance</returns>
        public Response<TPayload> Create() => new Response<TPayload>(payload, errors);

        internal BuildResponse<TPayload> WithPayload(TPayload payload)
        {
            this.payload = payload;
            return this;
        }
    }
}
