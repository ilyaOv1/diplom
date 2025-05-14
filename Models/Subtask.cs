namespace ProjManagmentSystem.Models
{
    public class Subtask
    {
        public int id { get; set; }
        public string name { get; set; }
        public int task { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string responsible { get; set; }
        public DateTime expected_date { get; set; }
        public bool IsUrgent =>
                                 (expected_date - DateTime.UtcNow).TotalDays <= 3 &&
                                 (expected_date - DateTime.UtcNow).TotalDays >= 0;

    }
}
