using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Transformation
{
    public class BlockChain : IEnumerable<Block>
    {
        public Block Start { get; protected set; }
        public Block End { get; protected set; }

        #region Public methods

        public BlockChain(Block start = null, Block end = null)
        {
            this.Start = start;
            this.End = end;
        }

        public int Count(string value)
        {
            return this.Count(block => block.Value == value);
        }

        /// <summary>
        /// Returns the copy of current expression part
        /// which blocks belong to the same expression too
        /// </summary>
        /// <returns></returns>
        public BlockChain Copy()
        {
            Expression expr = Start.Expression;

            Block copyStart = Block.New(expr, Start.Value);
            Block copyEnd = copyStart;

            for (Block current = Start.Next; current != End.Next; current = current.Next)
            {
                copyEnd = Block.New(expr, current.Value, copyEnd);
            }

            return new BlockChain(copyStart, copyEnd);
        }

        public void ExcludeParentheses()
        {
            string str = ToString();
            if (str.Length < 2)
                return;

            str = str.Substring(1, str.Length - 2);

            if (Start.Value == "(" && End.Value == ")" && str.IndexOf('(') <= str.IndexOf(')'))
            {
                Start = Start.Next;
                End = End.Prev;
            }
        }

        public void IncludeParentheses()
        {
            if (Start.Prev == null || End.Next == null)
                return;

            if (Start.Prev.Value == "(" && End.Next.Value == ")")
            {
                Start = Start.Prev;
                End = End.Next;
            }
        }

        /// <summary>
        /// Concatenates block from the given range into a one block (from).
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Collapse(Block from, Block to)
        {
            if (from == to)
                return;

            for (Block current = from.Next; current != to.Next; current = current.Next)
            {
                if (current.Prev.Value != "(" && current.Value != ")")
                {
                    from.Value += " ";
                }
                from.Value += current.Value;
            }

            from.Next = to.Next;
            if (to == End)
            {
                End = from;
            }
            else
            {
                from.Next.Prev = from;
            }
        }

        public void Insert(Block position, Block block)
        {
            if (position == null)
            {
                block.Next = Start;
                block.Prev = null;

                if (Start == null && End == null)
                {
                    Start = End = block;
                }
                else
                {
                    Start.Prev = block;
                    Start = block;
                }
                return;
            }

            block.Next = position.Next;
            block.Prev = position;
            position.Next = block;

            if (position == End)
                End = block;
            else
                block.Next.Prev = block;
        }

        public void InsertRange(Block position, Block from, Block to)
        {
            if (position == null)
            {
                Start.Prev = to;
                to.Next = Start;
                from.Prev = null;
                Start = from;
                return;
            }

            to.Next = position.Next;
            position.Next = from;
            from.Prev = position;

            if (position == End)
                End = to;
            else
                to.Next.Prev = to;
        }

        public void RemoveBlock(Block block)
        {
            /* Removing leading block */
            if (block == Start)
            {
                Start = Start.Next;
                if (Start != null)
                {
                    Start.Prev = null;
                }
            }

            /* Removing trailing block */
            else if (block == End)
            {
                End = End.Prev;
                End.Next = null;
            }

            /* Removing inner block */
            else
            {
                block.Prev.Next = block.Next;
                block.Next.Prev = block.Prev;
            }
        }

        public void RemoveRange(Block from, Block to)
        {
            if (from == Start)
            {
                Start = to.Next;
                if (Start != null)
                    Start.Prev = null;
            }
            else
                from.Prev.Next = to.Next;

            if (to == End)
            {
                End = from.Prev;
                if (End != null)
                    End.Next = null;
            }
            else
                to.Next.Prev = from.Prev;
        }

        public void SwapParts(BlockChain first, BlockChain second)
        {
            if (object.ReferenceEquals(first, second))
                return;

            Block firstOffset = first.Start.Prev;
            Block secondOffset = second.Start.Prev;

            RemoveRange(first.Start, first.End);
            RemoveRange(second.Start, second.End);

            InsertRange(firstOffset, second.Start, second.End);
            InsertRange(secondOffset, first.Start, first.End);
        }

        #endregion

        #region Operator overloads

        public static bool operator ==(BlockChain first, BlockChain second)
        {
            if (object.ReferenceEquals(first, second))
                return true;
            if ((object)first == null || (object)second == null)
                return false;

            Block i = first.Start;
            Block j = second.Start;

            while (i != first.End.Next || j != second.End.Next)
            {
                if (i.Value != j.Value)
                {
                    return false;
                }
                i = i.Next;
                j = j.Next;
            }
            return (i == first.End) && (j == second.End);
        }

        public static bool operator !=(BlockChain first, BlockChain second)
        {
            return !(first == second);
        }

        #endregion

        #region Interface implementations and overriden methods

        public override bool Equals(object obj)
        {
            BlockChain chain = obj as BlockChain;
            if (chain == null)
                return false;

            return (this == chain);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (var block in this)
            {
                result.Append(block.Value);

                if (block.Value == "(" || block == End ||
                   (block.Next != null && block.Next.Value == ")"))
                {
                    continue;
                }
                result.Append(' ');    
            }
            return result.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<Block> GetEnumerator()
        {
            for (Block current = Start; End != null && current != End.Next; current = current.Next)
                yield return current;
        }

        #endregion
    }
}
