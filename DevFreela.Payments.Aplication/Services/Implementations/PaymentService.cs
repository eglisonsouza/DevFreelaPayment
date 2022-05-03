using DevFreela.Payments.Aplication.Model;
using DevFreela.Payments.Aplication.Services.Interfaces;

namespace DevFreela.Payments.Aplication.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        public Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel)
        {
            return Task.FromResult(true);
        }
    }
}
