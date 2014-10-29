using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;

namespace TestLibrary
{
    /// <summary>
    /// 为解决Request.Browser["key"]
    /// </summary>
    public sealed class MockHttpBrowserCapabilities : HttpBrowserCapabilities
    {
        internal MockHttpBrowserCapabilities()
        {
            IDictionary list = new Dictionary<string, string>();
			//list.Add("requiresPostRedirectionHandling", "false");
            this.Items = list;
        }

		public IDictionary Items { get; private set; }

        public override string this[string key]
        {
            get
            {
                return (string)this.Items[key];
            }

        }

		new public string Browser
		{
			set { this.Set("browser", value); }
		}

		new public string Version
		{
			set { this.Set("version", value); }
		}

		public void Set(string key, string value)
		{
			this.Items[key] = value;
		}
    }
}
