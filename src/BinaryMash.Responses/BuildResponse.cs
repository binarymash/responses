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
        /// <typeparam name="T">The type of payload</typeparam>
        /// <returns>The payload-aware BuildResponse pipeline</returns>
        public static BuildResponse<T> WithPayload<T>() => new BuildResponse<T>();

        public static BuildResponseActivator<T> ActivatedFrom<T>()
            where T : class
        {
            return new BuildResponseActivator<T>();
        }

        /// <summary>
        /// Creates a response that contains a payload of the specified type
        /// </summary>
        /// <typeparam name="T">The type of payload</typeparam>
        /// <param name="payload">The payload</param>
        /// <returns>The BuildResponse pipeline</returns>
        public static BuildResponse<T> WithPayload<T>(T payload) => new BuildResponse<T>().WithPayload(payload);

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
    public class BuildResponse<T>
#pragma warning restore SA1402 // File may only contain a single class
    {
        private List<Error> errors;

        private T payload;

        internal BuildResponse()
        {
            payload = default(T);
            errors = new List<Error>();
        }

        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns>The payload-aware BuildResponse pipeline</returns>
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
        /// <returns>The payload-aware BuildResponse pipeline</returns>
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
        /// <returns>The Response instance</returns>
        public Response<T> Create() => new Response<T>(payload, errors);

        internal BuildResponse<T> WithPayload(T payload)
        {
            this.payload = payload;
            return this;
        }
    }
}
