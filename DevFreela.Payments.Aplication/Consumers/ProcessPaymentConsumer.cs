using DevFreela.Payments.Aplication.Model;
using DevFreela.Payments.Aplication.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DevFreela.Payments.Aplication.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        private const string QUEUE_PAYMENT = "Payments";
        private const string QUEUE_PAYMENT_APPROVED = "PaymentsApproved";

        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory ()
            {
                HostName = "localhost",
                Port = 6001,
            };

            _connection = factory.CreateConnection();
            
            _channel = _connection.CreateModel();

            QueueDeclare();
        }

        private IConnection CreateConnection()
        {
            return new ConnectionFactory
            {
                HostName = "localhost"
            }.CreateConnection();
        }

        private void QueueDeclare()
        {
            QueuePaymentDeclare();

            QueuePaymentApprovedDeclare();
        }

        private void QueuePaymentApprovedDeclare()
        {
            _channel.QueueDeclare
                (
                    queue: QUEUE_PAYMENT_APPROVED,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
        }

        private void QueuePaymentDeclare()
        {
            _channel.QueueDeclare
                (
                    queue: QUEUE_PAYMENT,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);

                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                ProcessPayment(paymentInfo!);

                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.IdProject);
                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish
                    (
                        exchange: String.Empty,
                        routingKey: QUEUE_PAYMENT_APPROVED,
                        basicProperties: null,
                        body: paymentApprovedBytes
                    );

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(QUEUE_PAYMENT, false, consumer);

            return Task.CompletedTask;
        }

        private void ProcessPayment(PaymentInfoInputModel paymentInfoInputModel)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                paymentService.Process(paymentInfoInputModel);
            }
        }
    }
}
