namespace Transformation
{
    public class EquivalentForm
    {
        public string Expression { get; private set; }
        public bool IsMinor { get; set; }
        public int GroupID { get; set; }

        public EquivalentForm(string expression, bool isMinor, int groupID)
        {
            this.Expression = expression;
            this.IsMinor = isMinor;
            this.GroupID = groupID;
        }
    }
}
