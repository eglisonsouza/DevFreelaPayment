using DevFreela.Payments.Aplication.Model;
using DevFreela.Payments.Aplication.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevFreela.Payments.API.Controllers
{
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(PaymentInfoInputModel paymentInfoInputModel)
        {
            if (!await _paymentService.Process(paymentInfoInputModel))
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
