namespace ODataRoutingSample.Models
{
    using System.Runtime.Serialization;

    public class FooTemplate
    {
        public string Id { get; set; }

        public Fizz Fizz { get; set; }

        [IgnoreDataMember]
        public bool FizzProvided { get; set; }

        public Buzz Buzz { get; set; }

        [IgnoreDataMember]
        public bool BuzzProvided { get; set; }
    }
}
