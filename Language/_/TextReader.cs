using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language
{
    internal class TextReader : IEnumerable<char>, IEnumerator<char>
    {
        private List<string> lines;
        private string currentLine;

        public Location CurrentLocation { get; private set; }

        public TextReader(string text)
        {
            this.lines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    this.lines.Add(line);
                    line = reader.ReadLine();
                }
            }
            this.Reset();
        }

        public string GetPart(Location from, Location to)
        {
            if (from.Line == to.Line)
                return this.lines[from.Line].Substring(from.Position, to.Position - from.Position);

            StringBuilder builder = new StringBuilder();
            builder.Append(this.lines[from.Line].Substring(from.Position));

            for (int line = from.Line + 1; line < to.Line; line++)
                builder.Append(this.lines[line]);

            builder.Append(this.lines[to.Line].Substring(0, to.Position));
            return builder.ToString();
        }

        #region IEnumerator & IEnumerable implementation

        public void Reset()
        {
            this.currentLine = this.lines.FirstOrDefault();
            this.CurrentLocation = new Location(0, -1);
        }

        public char Current
        {
            get
            {
                return this.currentLine[this.CurrentLocation.Position];
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public bool MoveNext()
        {
            if (string.IsNullOrEmpty(this.currentLine))
                return false;

            this.CurrentLocation = this.CurrentLocation.NextPosition();

            if (this.CurrentLocation.Position == this.currentLine.Length)
            {
                if (this.CurrentLocation.Line + 1 >= this.lines.Count)
                    return false;
                
                this.CurrentLocation = this.CurrentLocation.NextLine();
                this.currentLine = this.lines[this.CurrentLocation.Line];
            }
            return true;
        }

        public IEnumerator<char> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose() { }

        #endregion
    }
}
