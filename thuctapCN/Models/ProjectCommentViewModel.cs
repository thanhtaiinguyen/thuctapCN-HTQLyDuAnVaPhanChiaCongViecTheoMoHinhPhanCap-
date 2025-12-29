namespace thuctapCN.Models
{
    /// <summary>
    /// ViewModel hiển thị bình luận trong dự án
    /// </summary>
    public class ProjectCommentViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
