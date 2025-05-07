namespace ProjManagmentSystem.Models
{
    public class AddUsersToTaskDTO
    {
        public int taskId { get; set; }
        public List<UserWithResponsibilityDTO> userIds { get; set; }
    }
}
