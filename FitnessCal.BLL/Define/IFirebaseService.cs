using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IFirebaseService
    {
        Task<bool> SendNotificationAsync(string fcmToken, string title, string body);
    }
}
