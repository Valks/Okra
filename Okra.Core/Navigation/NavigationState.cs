using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Navigation
{
    [DataContract]
    public class NavigationState
    {
        // *** Constructors ***

        public NavigationState()
        {
            NavigationStack = new List<NavigationEntryState>();
        }

        // *** Properties ***

        [DataMember]
        public IList<NavigationEntryState> NavigationStack
        {
            get;
            private set;
        }
    }
}
