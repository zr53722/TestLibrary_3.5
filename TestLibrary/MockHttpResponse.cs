using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestLibrary
{
	public sealed class MockHttpResponse
	{
		private MockTextWriter _writer;

		internal MockHttpResponse(MockTextWriter writer)
		{
			_writer = writer;

		}

		public Encoding ContentEncoding
		{
			get { return _writer.StreamEncoding; }
			set { _writer.StreamEncoding = value; }
		}


		public Stream OutputStream
		{
			get { return _writer.Stream; }
		}

		public string GetText()
		{
			return _writer.GetText();
		}

	}
}
