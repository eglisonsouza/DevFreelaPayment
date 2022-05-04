using DevFreela.Payments.Aplication.Model;
using DevFreela.Payments.Aplication.Model.UI;
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

        public ProcessPaymentConsumer(IServiceProvider serviceProvider, ApiSettings apiSettings)
        {
            _serviceProvider = serviceProvider;

            _connection = CreateConnection(apiSettings);

            _channel = _connection.CreateModel();

            QueueDeclare();
        }

        private IConnection CreateConnection(ApiSettings apiSettings)
        {
            return new ConnectionFactory
            {
                HostName = apiSettings.Services.RabbitMQ
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
                try
                {
                    var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(GetPaymentInfoJson(eventArgs));

                    ProcessPayment(paymentInfo!);

                    PublishMessage(paymentInfo!);

                    _channel.BasicAck(eventArgs.DeliveryTag, false);

                }
                catch
                {
                    _channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(QUEUE_PAYMENT, false, consumer);

            return Task.CompletedTask;
        }

        private void PublishMessage(PaymentInfoInputModel paymentInfo)
        {
            _channel.BasicPublish
                (
                    exchange: string.Empty,
                    routingKey: QUEUE_PAYMENT_APPROVED,
                    basicProperties: null,
                    body: GetBody(paymentInfo!)
                );
        }

        private static string GetPaymentInfoJson(BasicDeliverEventArgs eventArgs)
        {
            return Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        }

        private static byte[] GetBody(PaymentInfoInputModel paymentInfo)
        {
            return new PaymentApprovedIntegrationEvent(paymentInfo!.IdProject).ToBytes();
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
