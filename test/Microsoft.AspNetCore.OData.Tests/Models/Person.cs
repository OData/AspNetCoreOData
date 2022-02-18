//-----------------------------------------------------------------------------
// <copyright file="Person.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.Tests.Models
{
    [DataContract]
    public class Person
    {
        [DataMember]
        public int Age { get; set; }

        [DataMember]
        public Gender Gender { get; set; }

        [DataMember(IsRequired = true)]
        public string FirstName { get; set; }

        [DataMember]
        public string[] Alias { get; set; }

        [DataMember]
        public Address Address { get; set; }

        [DataMember]
        public PhoneNumber HomeNumber { get; set; }

        public string UnserializableSSN { get; set; }

        [DataMember]
        public HobbyActivity FavoriteHobby { get; set; }

        [DataContract]
        public class HobbyActivity : IActivity
        {
            public HobbyActivity(string hobbyName)
            {
                this.ActivityName = hobbyName;
            }

            [DataMember]
            public string ActivityName
            {
                get;
                set;
            }

            public void DoActivity()
            {
                // Some Action
            }
        }
    }

    public interface IActivity
    {
        string ActivityName { get; set; }

        void DoActivity();
    }
}
