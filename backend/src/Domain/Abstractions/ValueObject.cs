using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregate
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();
        public override bool Equals(object? obj) =>
            obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        public override int GetHashCode() =>
            GetEqualityComponents().Aggregate(0, (hash, obj) => HashCode.Combine(hash, obj));
    }
}
