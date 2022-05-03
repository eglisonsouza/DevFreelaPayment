namespace DevFreela.Payments.Aplication.Model
{
    public class PaymentApprovedIntegrationEvent
    {
        public int IdProject { get; set; }

        public PaymentApprovedIntegrationEvent(int idProject)
        {
            IdProject = idProject;
        }
    }
}
