namespace ProjManagmentSystem.Models
{
    public class AddUsersToTaskDTO
    {
        public int TaskId { get; set; }
        public List<UserWithResponsibilityDTO> UserIds { get; set; }
    }
}
