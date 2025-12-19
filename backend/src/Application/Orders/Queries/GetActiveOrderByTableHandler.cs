using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries // Đảm bảo namespace đúng
{
    public sealed class GetActiveOrderByTableHandler
        : IQueryHandler<GetActiveOrderByTableQuery, OrderDto?>
    {
        private readonly IApplicationDbContext _db;
        public GetActiveOrderByTableHandler(IApplicationDbContext db) => _db = db;

        public async Task<OrderDto?> Handle(GetActiveOrderByTableQuery q, CancellationToken ct)
        {
            // 1. Lấy tất cả các đơn Active của bàn (Chưa Paid, Chưa Cancel)
            var activeOrders = await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.TableId == q.TableId
                            && o.OrderStatus != OrderStatus.Paid
                            && o.OrderStatus != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync(ct);

            // 2. LOGIC SỬA LỖI ĐƠN MA:
            // Tìm đơn hàng "Thật" - là đơn không phải Draft HOẶC là Draft nhưng đã có món.
            // Điều này giúp bỏ qua các đơn Draft rỗng (ghost orders) do hệ thống tự tạo.
            var order = activeOrders.FirstOrDefault(o =>
                o.OrderStatus != OrderStatus.Draft || o.Items.Count > 0);

            // 3. Fallback: Nếu không tìm thấy đơn nào "thật", thì mới trả về đơn Draft rỗng (nếu có) hoặc null
            order ??= activeOrders.FirstOrDefault();

            return order is null ? null : OrderMapper.ToDto(order);
        }
    }
}