namespace ProjManagmentSystem.Models
{
    public class Users
    {
        public string email { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string patronymic { get; set; }
        public string? description { get; set; }
        public bool? IsResponsible { get; set; }
        public DateTime? last_activity { get; set; }
    }
}
