using DevFreela.Payments.Aplication.Consumers;
using DevFreela.Payments.Aplication.Model.UI;
using DevFreela.Payments.Aplication.Services.Implementations;
using DevFreela.Payments.Aplication.Services.Interfaces;

const string SETTINGS = "Settings";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHostedService<ProcessPaymentConsumer>();
builder.Services.AddSingleton(builder.Configuration.GetSection(SETTINGS).Get<ApiSettings>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
