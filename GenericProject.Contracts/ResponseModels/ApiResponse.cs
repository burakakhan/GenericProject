using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenericProject.Contracts.ResponseModels
{
    public class ApiResponse<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Data yoksa gösterme
        public T? Data { get; private set; }
        public bool IsSuccess { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Hata yoksa gösterme
        public List<string>? Errors { get; private set; }

        [JsonIgnore] // Bu alanı JSON'da göstermeye gerek yok
        public int StatusCode { get; private set; }

        // Statik fabrika metotları
        public static ApiResponse<T> Success(T data, int statusCode = 200)
        {
            return new ApiResponse<T> { Data = data, IsSuccess = true, StatusCode = statusCode };
        }

        public static ApiResponse<T> Success(int statusCode = 204) // No Content gibi durumlar için
        {
            return new ApiResponse<T> { Data = default, IsSuccess = true, StatusCode = statusCode };
        }

        public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400)
        {
            return new ApiResponse<T> { Errors = errors, IsSuccess = false, StatusCode = statusCode };
        }

        public static ApiResponse<T> Fail(string error, int statusCode = 400)
        {
            return new ApiResponse<T> { Errors = new List<string> { error }, IsSuccess = false, StatusCode = statusCode };
        }

    }
}
