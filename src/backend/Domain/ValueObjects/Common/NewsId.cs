

namespace Domain.ValueObjects.Common
{
    public record NewsId(Guid Value)
    {
        public static NewsId New() => new(Guid.NewGuid());
        public static NewsId From(Guid value) => new(value);
        public override string ToString() => Value.ToString();
    }
}
