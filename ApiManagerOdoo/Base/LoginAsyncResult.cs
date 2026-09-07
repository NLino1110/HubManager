namespace ApiManagerOdoo.Base
{
    public enum LoginFailureKind
    {
        None,
        MissingConfig,
        Network,
        Server,
        Credentials,
        MissingSession
    }

    public class LoginAsyncResult
    {
        public bool Success { get; init; }
        public LoginFailureKind FailureKind { get; init; }
        public string UserMessage { get; init; } = string.Empty;
        public string DebugDetail { get; init; } = string.Empty;

        public static LoginAsyncResult Ok() => new() { Success = true };

        public static LoginAsyncResult Fail(
            LoginFailureKind kind,
            string userMessage,
            string debugDetail = "")
        {
            return new LoginAsyncResult
            {
                Success = false,
                FailureKind = kind,
                UserMessage = userMessage,
                DebugDetail = debugDetail
            };
        }
    }
}
