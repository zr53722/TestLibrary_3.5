using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Reflection;
using System.Globalization;
using System.Web.Hosting;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Principal;

namespace TestLibrary
{
	public sealed class WebContext : IDisposable
	{
		private HttpContext _context = null;
		
		/// <summary>
		/// 模拟的Request对象
		/// </summary>
		public MockHttpRequest Request { get; private set; }

		///// <summary>
		///// 模拟的Response对象
		///// </summary>
		public MockHttpResponse Response { get; private set; }

   
		private static readonly string STR_TestUrl = "http://www.test.com/test/abc.aspx?id=1&name=cc";


		public WebContext()
			: this(null)
		{
		}

		/// <summary>
		/// 构造函数，用于构造WEB的运行环境。
		/// </summary>
		/// <param name="url">一个绝对路径的URL字符串：/aa/bb/abc.aspx?id=2&name=xxxx</param>
		public WebContext(string url)
		{
			if( HttpContext.Current != null )
				throw new InvalidProgramException("TestContext 不支持嵌套使用。");

			MockHttpRuntime.EnsureInit();

			url = CheckUrl(url);


			HttpRequest request = CreateRequest(url ?? STR_TestUrl);
			Request = new MockHttpRequest(request);


			MockTextWriter writer = new MockTextWriter();
			HttpResponse response = new HttpResponse(writer);
            //解决Response.End()
            typeof(System.Web.HttpResponse).GetInstanceField("_flushing").SetValue(response, true);
			Response = new MockHttpResponse(writer);
			_context = new HttpContext(request, response);
			HttpContext.Current = _context;

            HttpContext.Current.ApplicationInstance = new HttpApplication();

			// 准备Session集合
			MockSessionState myState = new MockSessionState(Guid.NewGuid().ToString("N"),
															new SessionStateItemCollection(), new HttpStaticObjectsCollection(),
															5, true, HttpCookieMode.UseUri, SessionStateMode.InProc, false);

			HttpSessionState state = Activator.CreateInstance(typeof(HttpSessionState),
				 BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
				 new object[] { myState }, CultureInfo.CurrentCulture) as HttpSessionState;

			_context.Items["AspSession"] = state;



			// 准备Application集合
			SetApplicationState();
		}


		internal static HttpRequest CreateRequest(string url)
		{
			if( string.IsNullOrEmpty(url) )
				throw new ArgumentNullException("url");

			string path = null;
			string queryString = null;
			int p = url.IndexOf('?');
			if( p > 0 ) {
				path = url.Substring(0, p);
				queryString = url.Substring(p + 1);
			}
			else {
				path = url;
			}

            //TextWriter tw = new StringWriter();
            //HttpWorkerRequest wr = new System.Web.Hosting.SimpleWorkerRequest("/webapp", "c:\\inetpub\\wwwroot\\webapp\\", "default.aspx", "", tw);
            //HttpRequest hr = new HttpRequest(@"c:\web\test\abc.aspx", path, queryString);
            //hr.GetType().GetField("_wr", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(hr, wr);

            return new HttpRequest(@"c:\web\test\abc.aspx", path, queryString);
		}


		internal static string CheckUrl(string url)
		{
			if( string.IsNullOrEmpty(url) )
				return url;


			if( url.StartsWith("/") == false &&
					url.StartsWith("http://") == false &&
					url.StartsWith("https://") == false )
				throw new ArgumentException("参数 url 的格式无效。");


			if( url.StartsWith("http://") == false && url.StartsWith("https://") == false )
				return "http://www.test.com" + url;
			else
				return url;
		}


		private void SetApplicationState()
		{
			// HttpContext.Application 属性的实现如下：
			// return HttpApplicationFactory.ApplicationState;

			// HttpApplicationFactory.ApplicationState的实现（大致）如下：
			// return _theApplicationFactory._state;


			// 每次都创建一个新的容器对象，避免测试用例存在数据残留。
			HttpApplicationState appState = Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState;

			//得到HttpApplicationFactory并且给_state 赋值
			Type t = typeof(System.Web.HttpContext).Assembly.GetType("System.Web.HttpApplicationFactory");
			object httpApplicationFactoryInstance = Activator.CreateInstance(t, true);


			var theApplicationFactoryField = t.GetStaticField("_theApplicationFactory");
			theApplicationFactoryField.SetValue(null, httpApplicationFactoryInstance);


			var stateField = t.GetInstanceField("_state");
			stateField.SetValue(httpApplicationFactoryInstance, appState);
		}



		public void BindPage(System.Web.UI.Page page)
		{
			if( page == null )
				throw new ArgumentNullException("page");

			Type t = typeof(System.Web.UI.Page);

			t.GetInstanceField("_request").SetValue(page, _context.Request);
			t.GetInstanceField("_response").SetValue(page, _context.Response);
			t.GetInstanceField("_application").SetValue(page, _context.Application);
		}


		public void SetUserName(string username)
		{
			IIdentity _identity = new GenericIdentity(username);
			IPrincipal user = new GenericPrincipal(_identity, new string[0]);
			_context.User = user;
		}


		public void Dispose()
		{
			this._context = null;
			HttpContext.Current = null;
		}


	}





}

