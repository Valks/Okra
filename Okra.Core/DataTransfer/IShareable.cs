﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okra.DataTransfer
{
    public interface IShareable
    {
        void ShareRequested(IDataRequest dataRequest);
    }
}
