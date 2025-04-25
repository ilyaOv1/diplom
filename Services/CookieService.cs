namespace ProjManagmentSystem.Services
{
    public class CookieService
    {
        public void SaveCookie(HttpResponseMessage response, HttpResponse Response)
        {
            var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                Console.WriteLine("Полученные куки: " + cookieHeader);
                var cookies = cookieHeader.Split(',');
                foreach (var cookie in cookies)
                {
                    var parts = cookie.Split(';');
                    var keyValuePair = parts[0].Split('=');
                    if (keyValuePair.Length == 2)
                    {
                        var key = keyValuePair[0].Trim();
                        var value = keyValuePair[1].Trim();

                        Response.Cookies.Append(key, value, new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Path = "/"
                        });
                    }
                }

            }

        }
    }
}
