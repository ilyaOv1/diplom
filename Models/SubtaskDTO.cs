namespace ProjManagmentSystem.Models
{
    public class SubtaskDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ResponsibleEmail { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
