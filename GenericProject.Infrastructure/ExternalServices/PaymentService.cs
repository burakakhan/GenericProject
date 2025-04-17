using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Infrastructure.ExternalServices
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(decimal amount, string creditCardInfo);
    }

    public class PaymentService : IPaymentService
    {
        public Task<bool> ProcessPaymentAsync(decimal amount, string creditCardInfo)
        {
            // Örnek olarak ödeme işlemini burada yapıyoruz
            // Gerçek bir ödeme sistemi entegrasyonu yapılabilir
            Console.WriteLine($"Processing payment of {amount} with card info: {creditCardInfo}");
            return Task.FromResult(true);
        }
    }
}
