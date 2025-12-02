namespace Application
{
    public interface IOrderCodeGenerator
    {
        Task<string> GenerateAsync(Guid tableId, string tableCode, CancellationToken ct);
    }
}
