namespace UniHub.Application.DTOs.Common;

public record PaginatedResultDto<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
