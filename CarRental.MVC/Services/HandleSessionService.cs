using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CarRental.MVC.Services
{
    public class HandleSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HandleSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetObject<T>(string key, T value)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public T GetObject<T>(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
