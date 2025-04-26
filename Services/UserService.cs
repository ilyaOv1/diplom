namespace ProjManagmentSystem.Services
{
	public class UserService
	{
		public string email { get; set; }
		public string FIO { get; set; }
		public string Token { get; set; }

		public void SetUserData(string email, string fio, string token)
		{
			this.email = email;
			FIO = fio;
			Token = token;
		}

		public (string FIO, string Token) GetUserData()
		{
			return (FIO, Token);
		}
	}
}
