using System.Text;
using System.Text.Json;

namespace DevFreela.Payments.Aplication.Model
{
    public class PaymentApprovedIntegrationEvent
    {
        public int IdProject { get; set; }

        public PaymentApprovedIntegrationEvent(int idProject)
        {
            IdProject = idProject;
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(ToJson());
        }

        private string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
