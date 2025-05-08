namespace RealEstateApi.Models
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        //public T? Model { get; set; }
        public object? model { get; set; }
        public string message { get; set; } = "";
        public int code { get; set; }
    }

    public static class ApiResult
    {
        public static ApiResponse<T> Success<T>(T model, string message = "Thành công", int code = 200)
        {
            return new ApiResponse<T> { success = true, model = model, message = message, code = code };
        }

        public static ApiResponse<T> Error<T>(string message = "Thất bại", int code = 400)
        {
            return new ApiResponse<T> { success = false, model = default, message = message, code = code };
        }
    }
}
