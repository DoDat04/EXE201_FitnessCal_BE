namespace FitnessCal.BLL.Constants
{
    public static class ResponseCodes
    {
        // HTTP Status Codes
        public static class StatusCodes
        {
            public const int OK = 200;
            public const int CREATED = 201;
            public const int NO_CONTENT = 204;
            public const int BAD_REQUEST = 400;
            public const int UNAUTHORIZED = 401;
            public const int FORBIDDEN = 403;
            public const int NOT_FOUND = 404;
            public const int CONFLICT = 409;
            public const int UNPROCESSABLE_ENTITY = 422;
            public const int INTERNAL_SERVER_ERROR = 500;
        }

        // Response Messages
        public static class Messages
        {
            // Success Messages
            public const string SUCCESS = "Thành công";
            public const string CREATED = "Tạo thành công";
            public const string UPDATED = "Cập nhật thành công";
            public const string DELETED = "Xóa thành công";

            // Error Messages
            public const string BAD_REQUEST = "Yêu cầu không hợp lệ";
            public const string UNAUTHORIZED = "Không có quyền truy cập";
            public const string FORBIDDEN = "Truy cập bị từ chối";
            public const string NOT_FOUND = "Không tìm thấy";
            public const string CONFLICT = "Xung đột dữ liệu";
            public const string VALIDATION_ERROR = "Dữ liệu không hợp lệ";
            public const string INTERNAL_ERROR = "Lỗi hệ thống";
            public const string DATABASE_ERROR = "Lỗi cơ sở dữ liệu";
            public const string NETWORK_ERROR = "Lỗi kết nối";
        }
    }
} 