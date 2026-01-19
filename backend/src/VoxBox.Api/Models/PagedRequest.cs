namespace VoxBox.Api.Models;

public class PagedRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
    }

    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    public string? Filter { get; set; }

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}
