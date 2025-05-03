namespace ProjManagmentSystem.Models
{
    public class AddUsersToTaskDTO
    {
        public int TaskId { get; set; }
        public List<string> UserIds { get; set; }
    }
}
