namespace FitnessCal.BLL.Constants
{
    public class AuthMessage
    {
        // Login messages
        public const string LOGIN_FAILED = "Email hoặc mật khẩu không hợp lệ";
        public const string LOGIN_SUCCESS = "Đăng nhập thành công";
        public const string LOGIN_USER_NOT_FOUND = "Không tìm thấy người dùng với email đã cung cấp";
        public const string LOGIN_USER_NOT_ACTIVE = "Tài khoản người dùng bị chặn";

        // Register messages
        public const string REGISTER_EMAIL_EXISTS = "Email đã tồn tại";
        public const string REGISTER_SUCCESS = "Đăng ký thành công";
        public const string REGISTER_FAILED = "Đăng ký thất bại";

        // Refresh token messages
        public const string REFRESH_INVALID_TOKEN = "Refresh token không hợp lệ";
        public const string REFRESH_USER_NOT_FOUND = "Không tìm thấy người dùng";
        public const string REFRESH_SUCCESS = "Refresh token thành công";

        // Logout messages
        public const string LOGOUT_SUCCESS = "Đăng xuất thành công";
        public const string LOGOUT_INVALID_TOKEN = "Token không hợp lệ";
        public const string LOGOUT_FAILED = "Đăng xuất thất bại";
    }
}



