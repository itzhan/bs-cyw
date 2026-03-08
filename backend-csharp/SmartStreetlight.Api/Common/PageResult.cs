namespace SmartStreetlight.Api.Common;

public class PageResult<T>
{
    public List<T> Records { get; set; } = new();
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }

    public PageResult() { }

    public PageResult(List<T> records, long total, int page, int size)
    {
        Records = records;
        Total = total;
        Page = page;
        Size = size;
    }
}
