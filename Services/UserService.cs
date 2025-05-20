namespace ProjManagmentSystem.Services
{
	public class UserService
	{
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Email
        {
            get => _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");
            set => _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", value);
        }

        public string FIO
        {
            get => _httpContextAccessor.HttpContext?.Session.GetString("UserFIO");
            set => _httpContextAccessor.HttpContext?.Session.SetString("UserFIO", value);
        }

        public string Token
        {
            get => _httpContextAccessor.HttpContext?.Session.GetString("UserToken");
            set => _httpContextAccessor.HttpContext?.Session.SetString("UserToken", value);
        }

        public byte[]? Image
        {
            get
            {
                var imageBase64 = _httpContextAccessor.HttpContext?.Session.GetString("UserImage");
                return imageBase64 != null ? Convert.FromBase64String(imageBase64) : null;
            }
            set
            {
                var base64 = value != null ? Convert.ToBase64String(value) : null;
                _httpContextAccessor.HttpContext?.Session.SetString("UserImage", base64);
            }
        }

        public void SetUserData(string email, string fio, string token, byte[]? image)
        {
            Email = email;
            FIO = fio;
            Token = token;
            Image = image;
        }
    }
}
