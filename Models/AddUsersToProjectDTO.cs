namespace ProjManagmentSystem.Models
{
    public class AddUsersToProjectDTO
    {
        public int ProjectId { get; set; }
        public List<UserWithResponsibilityDTO> UserIds { get; set; }
    }
}
