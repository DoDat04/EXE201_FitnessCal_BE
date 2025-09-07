using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.PaymentDTO;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace FitnessCal.BLL.Implement
{
    public class PayosService : IPayosService
    {
        private readonly PayOS _payosClient;
        private readonly PayOSSettings _settings;

        public PayosService(IOptions<PayOSSettings> settings)
        {
            _settings = settings.Value;
            _payosClient = new PayOS(_settings.ClientId, _settings.ApiKey, _settings.ChecksumKey);
        }

        public async Task<PayOSPaymentResponse> CreatePaymentLinkAsync(CreatePayOSPaymentRequest request)
        {
            // Sử dụng orderCode từ request hoặc tạo mới
            var orderCode = request.OrderCode > 0 ? request.OrderCode : int.Parse(DateTimeOffset.Now.ToString("ffffff"));
           
            var payosItems = request.Items.Select(item => 
                new ItemData(item.Name, item.Quantity, (int)item.Price)).ToList();

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)request.Amount,
                description: request.Description,
                items: payosItems,
                returnUrl: request.ReturnUrl,
                cancelUrl: request.CancelUrl
            );

            var response = await _payosClient.createPaymentLink(paymentData);

            if (response == null || string.IsNullOrEmpty(response.checkoutUrl))
            {
                throw new InvalidOperationException("PayOS failed to create payment link");
            }

            return new PayOSPaymentResponse
            {
                OrderCode = orderCode,
                CheckoutUrl = response.checkoutUrl,
                Amount = request.Amount,
                Description = request.Description
            };
        }
    }
}
