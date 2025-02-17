//namespace OData2Linq.Tests.Issues29
//{
//    using System;
//    using System.Linq;
//    using System.Collections.Generic;
//    using Newtonsoft.Json;
//    using Community.OData.Linq; // V1.4.2
//    using Community.OData.Linq.Json; // V1.4.2
//    using Xunit;

//    /*********************************************************************************************/
//    /********************************** START Model Declaration **********************************/
//    /*********************************************************************************************/

//    public partial class URLT
//    {
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
//        public URLT()
//        {
//            this.DA = new HashSet<DA>();
//        }

//        public long URLTID { get; set; }
//        public string URLTN { get; set; }
//        public bool isDeleted { get; set; }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
//        public virtual ICollection<DA> DA { get; set; }
//    }

//    public partial class DA
//    {
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
//        public DA()
//        {
//            this.PDA = new HashSet<PDA>();
//        }

//        public long DAID { get; set; }
//        public long URLTID { get; set; }
//        public bool isDeleted { get; set; }

//        public virtual URLT URLT { get; set; }
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
//        public virtual ICollection<PDA> PDA { get; set; }
//    }

//    public partial class PDA
//    {
//        public long PDAID { get; set; }
//        public long DAID { get; set; }
//        public bool isDeleted { get; set; }
//        public long? nullTest { get; set; }

//        public virtual DA DA { get; set; }
//    }

//    /*********************************************************************************************/
//    /********************************** END Model Declaration ************************************/
//    /*********************************************************************************************/

//    public class Issue32
//    {
//        [Fact]
//        public void Expand()
//        {
//            // Expand - deep level 2
//            URLT URLTitem = new URLT { URLTID = 1, URLTN = "Test", isDeleted = false };

//            // Expand - deep level 1
//            DA DAitem = new DA { DAID = 1, URLT = URLTitem, isDeleted = false };

//            // Root Level
//            PDA[] items =
//                {
//                new PDA { PDAID = 1, DA = DAitem, isDeleted = false, nullTest = null },
//                new PDA { PDAID = 2, isDeleted = false, nullTest = null  },
//                new PDA { PDAID = 3, isDeleted = true, nullTest = null  }
//            };

//            // Filter working fine at root level
//            string filter = "(PDAID eq 1 or PDAID eq 3) and isDeleted eq false and DA/URLT/isDeleted eq true"; // Filter root - Case 1
//                                                                                 //string filter = "(PDAID eq 1 or PDAID eq 3) and isDeleted eq true"; // Filter root - Case 2
//            string orderBy = "PDAID desc";
//            string selectStr = "PDAID,IsDeleted,nullTest";
//            // Filter inside expand isn't working all the time
//            // to replicate please use the following cases 
//            //string ExpandStr = "DA($select=DAID,isDeleted;$Filter=isDeleted eq false;$expand=URLT($Select=*;$filter=isDeleted eq false))"; // Case 1 - Expand Filter working fine (Aparently)
//            string ExpandStr = "DA($select=DAID,isDeleted;$Filter=isDeleted eq false;$expand=URLT($Select=*;$Filter=isDeleted eq true))"; // Case 2 - Expand Filter not working
//                                                                                                                                          // string ExpandStr = "DA($select=DAID,isDeleted;$Filter=isDeleted eq true;$expand=URLT($Select=*;$filter=isDeleted eq true))"; // Case 3 - Expand Filter not working fine
//                                                                                                                                          // string ExpandStr = "DA($select=DAID,isDeleted;$Filter=isDeleted eq true;$expand=URLT($Select=*;$filter=URLTN eq 'a'))"; // Case 4 - Expand Filter not working fine on diferent DataTypes

//            ODataQuery<PDA> query = items.AsQueryable().OData();

//            ODataSettings settings = (ODataSettings)query.ServiceProvider.GetService(typeof(ODataSettings));
//            Assert.True(settings.EnableCaseInsensitive);

//            var result = query.OData().Filter(filter).OrderBy(orderBy).SelectExpand(selectStr, ExpandStr).ToJson(settings => settings.NullValueHandling = NullValueHandling.Ignore);
//            string str = result.ToString();
//            Console.WriteLine(str);
//            Console.WriteLine(Environment.NewLine);

//        }
//    }
//}