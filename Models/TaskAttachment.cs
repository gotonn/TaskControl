using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models;

public class TaskAttachment
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    [Required]
    [StringLength(220)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(600)]
    public string FilePath { get; set; } = string.Empty;

    public string UploadedById { get; set; } = string.Empty;

    public ApplicationUser UploadedBy { get; set; } = null!;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
