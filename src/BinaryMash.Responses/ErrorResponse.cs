using System.Collections.Generic;

namespace BinaryMash.Responses
{
    public class ErrorResponse : Response
    {
        public ErrorResponse(IEnumerable<Error> errors)
        {
            AddErrors(errors);
        }
    }
}
