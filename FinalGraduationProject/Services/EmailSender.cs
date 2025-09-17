using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace FinalGraduationProject.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // مؤقتًا: اطبع الإيميل في الكونسول
            Console.WriteLine($"Email To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");

            // ترجع Task جاهزة بدل ما تبعت فعليًا
            return Task.CompletedTask;
        }
    }
}
