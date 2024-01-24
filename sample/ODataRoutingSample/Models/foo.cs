namespace ODataRoutingSample.Models
{
    using System.Runtime.Serialization;

    public class FooProperties
    {
        public Fizz Fizz { get; set; }

        [IgnoreDataMember]
        public bool FizzProvided { get; set; }

        public Buzz Buzz { get; set; }

        [IgnoreDataMember]
        public bool BuzzProvided { get; set; }

        public Frob Frob { get; set; }

        [IgnoreDataMember]
        public bool FrobProvided { get; set; }
    }

    public class Foo
    {
        public string Id { get; set; }

        public Fizz Fizz { get; set; }

        public Buzz Buzz { get; set; }

        public Frob Frob { get; set; }
    }

    public class Fizz
    {
    }

    public class Buzz
    {
    }

    public class Frob
    {
    }
}
