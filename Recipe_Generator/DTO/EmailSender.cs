using Recipe_Generator.Interface;
using System.Net;
using System.Net.Mail;

namespace Recipe_Generator.DTO
{
    public class EmailSender : IEmailSender
    {
        
        protected readonly string sender = "RecipeIQ.devs@gmail.com";
        protected readonly string password = "mznr fmde xlno qivh";

        public Task SendEmail(string email,string name)
        {
            var subject = "Greetings";
            var message = "Dear "+  name +
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

            var client = new SmtpClient("smtp.gmail.com",587)
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

        public Task SendEmailNotification(string email, string name , string comment , DateTime time ,string username,string msg)
        {
            var subject = "Notification";
            var message = "Dear "+ name +
                "\r\n\r\nYou have received a new "+ msg +" on your RecipeIQ account. " +
                "Here are the details:" +
                "\r\n\r\n" +msg+ ": "+ comment +
                "\r\n\r\nPosted By: " + username +
                "\r\n\r\nDate and Time: " + time +
                "\r\n\r\nTo view and respond to the comment, please log in to your RecipeIQ account." +
                "\r\n\r\nThank you for staying engaged with our community!" +
                "\r\n\r\nBest regards,RecipeIQ family" +
                "\r\nRecipeIQ.devs@gmail.com";

            var client = new SmtpClient("smtp.gmail.com", 587)
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
