using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestLibrary
{

	public sealed class MockTextWriter : System.IO.TextWriter, IDisposable
	{
		//internal MockTextWriter() { }

		private MemoryStream _stream = new MemoryStream();

		internal Encoding StreamEncoding = Encoding.UTF8;

		public override Encoding Encoding
		{
			get { return this.StreamEncoding; }
		}



		public override void Write(string value)
		{
			Byte[] buffer = this.StreamEncoding.GetBytes(value);
			_stream.Write(buffer, 0, buffer.Length);
		}


		public Stream Stream
		{
			get { return _stream; }
		}


		public string GetText()
		{
			Stream.Position = 0;

			byte[] buffer = new byte[Stream.Length];
			Stream.Read(buffer, 0, buffer.Length);

			return StreamEncoding.GetString(buffer);
		}



		void IDisposable.Dispose()
		{
			_stream.Dispose();
		}

	}
}
