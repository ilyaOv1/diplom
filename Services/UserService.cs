namespace ProjManagmentSystem.Services
{
	public class UserService
	{
		public string email { get; set; }
		public string FIO { get; set; }
		public string Token { get; set; }
		public byte[]? image { get; set; }

		public void SetUserData(string email, string fio, string token, byte[]? image)
		{
			this.email = email;
			FIO = fio;
			Token = token;
			this.image = image;
		}

		public (string FIO, byte[]? image, string Token) GetUserData()
		{
			return (FIO, image, Token);
		}
	}
}
