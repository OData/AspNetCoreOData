namespace ODataQueryBuilder.Query.Container
{
    internal class SingleExpandedProperty<T> : NamedProperty<T>
    {
        public bool IsNull { get; set; }

        public override object GetValue()
        {
            return IsNull ? (object)null : Value;
        }
    }
}
