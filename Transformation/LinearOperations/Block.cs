namespace Transformation
{
    public class Block
    {
        public Expression Expression { get; private set; }
        public string Value { get; set; }

        public Block Next { get; set; }
        public Block Prev { get; set; }

        private Block() { }

        public override string ToString()
        {
            return Value;
        }

        public static Block New(Expression expr, string value, Block prev = null, Block next = null)
        {
            var result = new Block
            {
                Expression = expr,
                Value = value,
                Prev = prev,
                Next = next
            };

            if (prev != null)
            {
                prev.Next = result;
            }
            if (next != null)
            {
                next.Prev = result;
            }

            return result;
        }
    }
}
