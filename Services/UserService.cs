namespace ProjManagmentSystem.Services
{
	public class UserService
	{
		public string FIO { get; set; }
		public string Token { get; set; }

		public void SetUserData(string fio, string token)
		{
			FIO = fio;
			Token = token;
		}

		public (string FIO, string Token) GetUserData()
		{
			return (FIO, Token);
		}
	}
}
