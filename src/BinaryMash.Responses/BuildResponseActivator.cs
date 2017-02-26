namespace BinaryMash.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BuildResponseActivator<TResponse>
        where TResponse : class
    {
        private List<Error> errors;

        private object payload;

        internal BuildResponseActivator()
        {
            payload = null;
            errors = new List<Error>();
        }

        public BuildResponseActivator<TResponse> AndWithPayload(object payload)
        {
            this.payload = payload;
            return this;
        }

        /// <summary>
        /// Adds an error to the response
        /// </summary>
        /// <param name="error">An error</param>
        /// <returns>The payload-aware BuildResponse pipeline</returns>
        public BuildResponseActivator<TResponse> AndWithErrors(Error error)
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
        public BuildResponseActivator<TResponse> AndWithErrors(IEnumerable<Error> errors)
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
        public TResponse Create()
        {
            if (HasPayload(typeof(TResponse)))
            {
                return Activator.CreateInstance(typeof(TResponse), new object[] { payload, errors }) as TResponse;
            }
            else
            {
                return Activator.CreateInstance(typeof(TResponse), new[] { errors }) as TResponse;
            }
        }

        private bool HasPayload(Type responseType)
        {
            return responseType.GenericTypeArguments.Any();
        }

        private Type PayloadType(Type responseType)
        {
            return responseType.GenericTypeArguments[0];
        }
    }
}