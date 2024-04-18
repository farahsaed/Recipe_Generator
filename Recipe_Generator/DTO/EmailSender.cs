using Recipe_Generator.Interface;
using System.Net;
using System.Net.Mail;

namespace Recipe_Generator.DTO
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        
        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private string Sender()
        {
            string sender = configuration.GetSection("EmailData").GetSection("Email").Value;
            return sender;
        }
        private string Password()
        {
            string password = configuration.GetSection("EmailData").GetSection("Password").Value;
            return password;
        }
        private string SMTPHost()
        {
            string smtpHost = configuration.GetSection("EmailData").GetSection("SMTPHost").Value;
            return smtpHost;
        }
        private int SMTPPort()
        {
            var smtpHost = Convert.ToInt32(configuration.GetSection("EmailData").GetSection("SMTPPort").Value);
            return smtpHost;
        }
        private string GreetingMessage(string name)
        {
            string message = "Dear " + name +
                "\r\n\r\nWe hope this email finds you well and surrounded by the comforting aromas of your favorite dishes." +
                " As the seasons change, we wanted to take a moment to express our gratitude for being a cherished part of RecipeIQ family." +
                "\r\n\r\nIn the spirit of joy and good food, we've curated a collection of heartfelt greetings and culinary inspiration to brighten your day:" +
                "\r\n\r\nSeasonal Delights: Embrace the flavors of the season with our handpicked recipes designed to bring warmth to your table." +
                "\r\n\r\nCooking Together: We believe that the best meals are made with love and shared with loved ones. " +
                "Explore our cooking tips and ideas to create memorable moments in your kitchen." +
                "\r\n\r\nCommunity Spotlight: Discover stories from our diverse community of home cooks. " +
                "Your culinary journey is unique, and we love celebrating it with you." +
                "\r\n\r\nWe invite you to visit our website [Website URL] to explore these heartwarming greetings and find inspiration for your next culinary adventure." +
                "\r\n\r\nThank you for being a part of our flavorful community. " +
                "Your passion for cooking fuels our dedication to providing you with the best recipes, tips, and experiences." +
                "\r\n\r\nWishing you a season filled with delicious moments and shared joy!" +
                "\r\n\r\nWarm regards, RecipeIQ family\r\n" +
                "RecipeIQ.devs@gmail.com";
            return message;
         }
        private string NotificationMessage(string name , string msg ,
                                           string comment, string username ,
                                           DateTime time) 
        {
            string message ="Dear "+ name +
                            "\r\n\r\nYou have received a new "+ msg +" on your RecipeIQ account. " +
                            "Here are the details:" +
                            "\r\n\r\n" +msg+ ": "+ comment +
                            "\r\n\r\nPosted By: " + username +
                            "\r\n\r\nDate and Time: " + time +
                            "\r\n\r\nTo view and respond to the comment, please log in to your RecipeIQ account." +
                            "\r\n\r\nThank you for staying engaged with our community!" +
                            "\r\n\r\nBest regards,RecipeIQ family" +
                            "\r\nRecipeIQ.devs@gmail.com";
            return message;
        }

        public Task SendEmailGreeting(string email,string name)
        {
            var sender = Sender();
            var password = Password();
            var subject = "Greetings";
            var message = GreetingMessage(name);
            var smtpHost = SMTPHost();
            var smtpPort = SMTPPort();

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(sender, password)
            };

            client.UseDefaultCredentials = false;

            return client.SendMailAsync(
                new MailMessage(
                    from: sender,
                    to: email,
                    subject: subject,
                    message
                    )
                );
        }
        public Task SendEmailNotification(string email, string name ,
                                          string comment , DateTime time ,
                                          string username,string msg)
        {
            var sender = Sender();
            var password = Password();
            var subject = "Notification";
            var message = NotificationMessage(name , msg,comment,username,time);
            var smtpHost = SMTPHost();
            var smtpPort = SMTPPort();

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(sender, password)
            };

            client.UseDefaultCredentials = false;

            return client.SendMailAsync(
                new MailMessage(
                    from : sender,
                    to : email,
                    subject: subject,
                    message
                    ));
        }

    }
}
