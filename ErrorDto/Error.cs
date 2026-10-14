public class AppError : Exception
{
    public int StatusCode {get; set;}
    public AppError(int StatusCode, string Message) : base(Message)
    {
        this.StatusCode = StatusCode;
    }
}