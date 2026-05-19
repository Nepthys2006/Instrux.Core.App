using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record ContentItemDto(int Id, int ClassId, int? FolderId, string Title, string? Description, ContentType Type, string FilePath, DateTime UploadedAt, bool IsVisible);
