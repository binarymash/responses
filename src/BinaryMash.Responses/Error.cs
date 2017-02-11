namespace BinaryMash.Responses
{
    public class Error
    {
        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; private set; }

        public string Message { get; private set; }
    }
}
