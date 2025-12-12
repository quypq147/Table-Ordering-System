namespace WaiterApp;

public static class WaiterApiEndpoints
{
    public static class Auth
    {
        // POST: /api/auth/login
        public const string Login = "api/auth/login";
    }

    public static class Orders
    {
        // GET: /api/orders/{id}
        public static string Get(Guid id) => $"api/orders/{id}";

        // GET: /api/orders/summaries?page=1&pageSize=200
        public static string Summaries(int page, int pageSize)
            => $"api/orders/summaries?page={page}&pageSize={pageSize}";

        // POST: /api/orders/start
        // Body: { orderId, tableId }
        public const string Start = "api/orders/start";

        // POST: /api/orders/{id}/submit
        public static string Submit(Guid id) => $"api/orders/{id}/submit";

        // POST: /api/orders/{id}/in-progress
        public static string InProgress(Guid id) => $"api/orders/{id}/in-progress";

        // POST: /api/orders/{id}/ready
        public static string Ready(Guid id) => $"api/orders/{id}/ready";

        // POST: /api/orders/{id}/served
        public static string Served(Guid id) => $"api/orders/{id}/served";

        // POST: /api/orders/{id}/cancel
        public static string Cancel(Guid id) => $"api/orders/{id}/cancel";

        // POST: /api/orders/{id}/pay   (body: PayDto)
        public static string Pay(Guid id) => $"api/orders/{id}/pay";

        // POST: /api/orders/{id}/items (body: AddItemDto)
        public static string AddItem(Guid id) => $"api/orders/{id}/items";

        // PATCH: /api/orders/{id}/items/{orderItemId}
        public static string ChangeItemQuantity(Guid id, int orderItemId)
            => $"api/orders/{id}/items/{orderItemId}";

        // DELETE: /api/orders/{id}/items (body: { orderItemId })
        public static string RemoveItem(Guid id) => $"api/orders/{id}/items";

        // GET: /api/orders?tableId={tableId}&page=1&pageSize=20
        public static string ByTable(Guid tableId, int page = 1, int pageSize = 20)
            => $"api/orders?tableId={tableId}&page={page}&pageSize={pageSize}";

        // GET: /api/orders/table/{tableId}
        public static string ItemsByTable(Guid tableId)
            => $"api/orders/table/{tableId}";
    }

    public static class Tables
    {
        // GET: /api/tables
        public const string List = "api/tables";

        // GET: /api/tables/{id}
        public static string Get(Guid id) => $"api/tables/{id}";

        // POST: /api/tables/{id}/occupied
        public static string MarkOccupied(Guid id) => $"api/tables/{id}/occupied";

        // POST: /api/tables/{id}/available
        public static string MarkAvailable(Guid id) => $"api/tables/{id}/available";
    }

    public static class Kds
    {
        // Nếu sau này WaiterApp cần REST cho bếp:
        // ví dụ: GET /api/kds/tickets ...
    }

    public static class Payments
    {
        // POST: /api/payments/confirm-cash
        // Body: { orderId, amount }
        public const string ConfirmCash = "api/payments/confirm-cash";
    }
}

