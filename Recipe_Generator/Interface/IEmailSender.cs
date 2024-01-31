namespace Recipe_Generator.Interface
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email,string name);
    }
}
