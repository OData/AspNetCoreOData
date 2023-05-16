namespace QueryBuilder.Query.Container
{
    internal class AutoSelectedNamedProperty<T> : NamedProperty<T>
    {
        public AutoSelectedNamedProperty()
        {
            AutoSelected = true;
        }
    }
}
