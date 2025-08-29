using FitnessCal.BLL.DTO.PaymentDTO;

namespace FitnessCal.BLL.Define
{
    public interface IPayosService
    {
        Task<PayOSPaymentResponse> CreatePaymentLinkAsync(CreatePayOSPaymentRequest request);
    }
}
