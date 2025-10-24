using Domain.Enums;

namespace Domain.Entities
{
    public class Voucher
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; } = default!;
        public decimal Discount { get; private set; }
        public DiscountType DiscountType { get; private set; }
        public DateOnly CreatedAt { get; private set; }
        public DateOnly ExpirationDate { get; private set; }

        private Voucher() { } // EF

        public Voucher(Guid id, string code, decimal discount, DiscountType discountType, DateOnly createdAt, DateOnly expirationDate)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentNullException(nameof(code));
            if (discount <= 0) throw new ArgumentOutOfRangeException(nameof(discount));

            Id = id;
            Code = code.Trim();
            Discount = discount;
            DiscountType = discountType;
            CreatedAt = createdAt;
            ExpirationDate = expirationDate;
        }
    }
}

