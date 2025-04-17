using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Infrastructure.ExternalServices
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
    public class EmailSender : IEmailSender
    {

        public Task SendEmailAsync(string to, string subject, string body)
        {
            // Email gönderme işlemi burada yapılacak
            // Örnek olarak Console'a yazdırıyoruz
            Console.WriteLine($"Email sent to {to} with subject: {subject}");
            return Task.CompletedTask;
        }
    }
}