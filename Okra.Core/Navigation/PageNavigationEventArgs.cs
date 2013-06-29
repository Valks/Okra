﻿using Okra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Okra.Navigation
{
    public class PageNavigationEventArgs : EventArgs
    {
        // *** Fields ***

        private readonly INavigationEntry page;
        private readonly NavigationMode navigationMode;

        // *** Constructors ***

        public PageNavigationEventArgs(INavigationEntry page, NavigationMode navigationMode)
        {
            // Validate arguments

            if (page == null)
                throw new ArgumentNullException("page");

            if (!Enum.IsDefined(typeof(NavigationMode), navigationMode))
                throw new ArgumentException(ResourceHelper.GetErrorResource("Exception_ArgumentException_SpecifiedEnumIsNotDefined"), "navigationMode");

            // Set properties

            this.page = page;
            this.navigationMode = navigationMode;
        }

        // *** Properties ***

        public NavigationMode NavigationMode
        {
            get
            {
                return navigationMode;
            }
        }

        public INavigationEntry Page
        {
            get
            {
                return page;
            }
        }
    }
}
