using AdminWeb.Dtos;

namespace AdminWeb.Services
{
    public interface IBackendApiClient
    {
        // MENU
        Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default);
        Task<HttpResponseMessage> CreateMenuItemAsync(CreateMenuItemRequest req, CancellationToken cancellationToken = default);

        // ORDERS
        Task<Paginated<OrderSummaryDto>> GetOrdersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<OrderDetailDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> UpdateOrderStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);

        // KDS
        Task<List<KitchenTicketDto>> GetTicketsAsync(Guid stationId, CancellationToken cancellationToken = default);
        // TABLES
        Task<List<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> CreateTableAsync(CreateTableRequest req, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> UpdateTableAsync(Guid id, UpdateTableRequest req, CancellationToken cancellationToken = default);

    }
}
