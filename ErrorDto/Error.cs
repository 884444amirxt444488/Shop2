/*
public class AppError : Exception {
    public int StatusCode {get;}

    public AppError(int StatusCode, string Message) : base(Message)
    {
        this.StatusCode = StatusCode;
    }
}
*/

public class AppError : Exception
{
    public int StatusCode {get;}
    public AppError(int StatusCode, string Message) : base(Message)
    {
        this.StatusCode = StatusCode;
    }
}








