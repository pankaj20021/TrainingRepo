namespace SchoolProject.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = null)
            => new ApiResponse<T> { Data = data, Message = message ?? "Request successful" };

        public static ApiResponse<T> Fail(string message)
            => new ApiResponse<T> { Success = false, Message = message };
    }
}
