using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record CreateContentItemDto(int ClassId, int? FolderId, string Title, string? Description, ContentType Type, string FilePath, bool IsVisible);
