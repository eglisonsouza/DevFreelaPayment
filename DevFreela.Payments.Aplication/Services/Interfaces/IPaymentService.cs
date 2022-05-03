using DevFreela.Payments.Aplication.Model;

namespace DevFreela.Payments.Aplication.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel);
    }
}
