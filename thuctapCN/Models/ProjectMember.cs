using System.ComponentModel.DataAnnotations;

namespace thuctapCN.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty; // "Manager" or "Member"

        [Display(Name = "Ngày tham gia")]
        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Project Project { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
