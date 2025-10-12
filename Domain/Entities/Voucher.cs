using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Voucher
    {
        public Guid Id { get; set; }
        public string Code { get; set; }

        public decimal Discount { get; set; }
        public DiscountType DiscountType { get; set; }

        public DateOnly CreatedAt { get; set; }

        public DateOnly ExpirationDate { get; set; }
    }
}
