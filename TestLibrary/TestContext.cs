using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TestLibrary
{
	public sealed class TestContext : IDisposable
	{
		public TestContext()
		{
			if( HttpContext.Current != null )
				throw new InvalidProgramException("TestContext 不支持嵌套使用。");



			throw new NotImplementedException();
		}

		public MockHttpRequest Request { get; private set; }

		public MockHttpResponse Response { get; private set; }

		public MockHttpRuntime HttpRuntime { get; private set; }


		/// <summary>
		/// 给 HttpContext.Item 集合添加数据。
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddContextItem(object name, object value)
		{

		}
		


		public void Dispose()
		{
			throw new NotImplementedException();
		}

	}


}

