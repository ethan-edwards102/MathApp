namespace MathApiClient.Util;

public sealed class AuthLogger
{
    private readonly StreamWriter _fileWriter;
        
    private static readonly AuthLogger _instance = new AuthLogger();
    
    static AuthLogger() { }

    private AuthLogger()
    {
        _fileWriter = new StreamWriter("auth_errors.log", true);
        _fileWriter.AutoFlush = true;
    }
    
    public static AuthLogger Instance
    {
        get
        {
            return _instance;
        }
    }

    public void LogError(string message)
    {
        string logMessage = $"[{DateTime.Now}] - ERROR: {message}";
        _fileWriter.WriteLine(logMessage);
    }
}