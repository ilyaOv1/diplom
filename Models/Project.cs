namespace ProjManagmentSystem.Models
{
    public class Project
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string creatorProj { get; set; }
        public string creatorName { get; set; }
        public bool Access { get; set; }
    }
}
