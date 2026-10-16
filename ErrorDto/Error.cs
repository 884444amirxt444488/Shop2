public class AppError : Exception
{
    public int StatusCode;
    public AppError(int StatusCode, string Message) : base(Message)
    {
        this.StatusCode = StatusCode;
    }
}