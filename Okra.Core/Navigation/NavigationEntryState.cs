using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Navigation
{
    [DataContract]
    public class NavigationEntryState
    {
        // *** Constructors ***

        public NavigationEntryState(string pageName, byte[] argumentsData, byte[] stateData)
        {
            this.PageName = pageName;
            this.ArgumentsData = argumentsData;
            this.StateData = stateData;
        }

        // *** Properties ***

        [DataMember]
        public string PageName { get; private set; }

        [DataMember]
        public byte[] ArgumentsData { get; private set; }

        [DataMember]
        public byte[] StateData { get; private set; }
    }
}
