namespace ProjManagmentSystem.Models
{
    public class Tasks
    {
        public int id { get; set; }
        public string name { get; set; }
        public int project { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string creator { get; set; }
        public DateTime expected_date { get; set; }
        
    }
}
