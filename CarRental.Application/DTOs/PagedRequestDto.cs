namespace CarRental.Application.DTOs;

public class PagedRequestDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string SortBy { get; set; }
    public string sortDirection { get; set; }
}