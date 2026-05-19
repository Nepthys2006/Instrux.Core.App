using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class ContentItem
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public int? FolderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContentType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsVisible { get; set; }
}
