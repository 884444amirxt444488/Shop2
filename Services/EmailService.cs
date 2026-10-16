
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


public class SendMail
{
    private readonly IConfiguration _configuration;

    public SendMail(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendCode(string UserEmail, string Code)
    {
        var Message = new MimeMessage();

        string Username = _configuration["EmailSetting:Username"] ?? throw new InvalidOperationException("Username not found");
        string Password = _configuration["EmailSetting:Password"] ?? throw new InvalidOperationException("Password not found");



        Message.From.Add(new MailboxAddress("ShopApi", Username));
        Message.To.Add(MailboxAddress.Parse(UserEmail));

        Message.Subject = "Code to change your forgotten password";
        Message.Body = new TextPart("html")
        {
            Text = $"""
                    <div style="background: linear-gradient(
                        180deg,
                        rgb(0, 0, 154),
                        rgb(141, 1, 129)
                    );
                    color: white;
                    text-align: center;
                    border-radius: 20px;
                    width: 80%;
                    max-width: 500px;
                    min-width: 200px;
                    margin: 100px auto;
                    border: 2px solid rgb(0, 234, 255);
                    ">
                        <h2 style="background-color: rgb(255, 242, 0);
                            color: rgb(1, 1, 1);
                            padding: 5px;
                            border-top: 2px solid white;
                            border-bottom: 2px solid white;
                        ">****Hello****</h2>
                        <p>I am sender your code</p>
                        <p>Do not share this code with any body</p>
                        <h3>Your code: {Code}</h3>  
                    </div>

            """
        };

        using var Client = new SmtpClient();

        await Client.ConnectAsync(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls
        );

        await Client.AuthenticateAsync(Username, Password);
        await Client.SendAsync(Message);
        await Client.DisconnectAsync(true);
    }
}










