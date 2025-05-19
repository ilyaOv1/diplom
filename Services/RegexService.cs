using System.Text.RegularExpressions;

namespace ProjManagmentSystem.Services
{
    public class RegexService
    {
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        public static bool IsValidLatinName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string pattern = @"^[A-Za-z\s\-]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(value);
        }

        public static bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(password);
        }
    }
}
