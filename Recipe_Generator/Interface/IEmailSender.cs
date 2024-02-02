namespace Recipe_Generator.Interface
{
    public interface IEmailSender
    {
        Task SendEmail(string email,string name);
        Task SendEmailNotification(string email,string name, string comment, DateTime time, string username,string msg);
       
    }
}
