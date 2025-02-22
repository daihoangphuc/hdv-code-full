using System.Text.Json;

namespace HTSV.FE.Services
{
    public interface ISessionService
    {
        void Set<T>(string key, T value);
        T? Get<T>(string key);
        void Remove(string key);
        void Clear();
    }

    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Set<T>(string key, T value)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(key, JsonSerializer.Serialize(value));
            }
        }

        public T? Get<T>(string key)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var value = session?.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public void Remove(string key)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }

        public void Clear()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Clear();
        }
    }
} 