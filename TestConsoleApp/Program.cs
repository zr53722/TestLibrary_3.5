using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestLibrary;
using System.Web;
using SmokingTestLibrary;
using System.IO;
using System.Xml;

namespace TestConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{

    
			string appPath = @"c:\aaa\bb";
			MockHttpRuntime.AppDomainAppPath = appPath;
			MockHttpRuntime.AppDomainAppVirtualPath = "/";


			TestPage page = new TestPage();

			using( WebContext context = new WebContext("/pages/abc.aspx?id=2&name=aa") ) {
				// 准备WEB环境，用于测试


                HttpApplication sender = HttpContext.Current.ApplicationInstance;

             
               
                var a = HttpContext.Current.Request.ServerVariables;
				context.SetUserName("fish-li");
				context.Request.Browser.Browser = "IE";
				context.Request.Browser.Version = "7.0";
				context.Request.UrlReferrer  ="/a2.aspx?cc=5";
				context.Request.RequestType = "POST";

				Encoding contentEncoding = Encoding.Default;
				context.Request.ContentEncoding = contentEncoding;
				context.Request.ContentType ="text";

				string formData = "a=1&b=2&c=3";
				context.Request.SetForm(formData);
				context.Request.AddHeader("h1", "111111111");
				context.Request.AddHeader("h2", "22222222");
				context.Request.AddCookie(new HttpCookie("cookie1", "cccccccccc"));

				context.BindPage(page);

            

	
				//Console.WriteLine("PathInfo: " + HttpContext.Current.Request.PathInfo);
				//Console.WriteLine("PhysicalPath: " + HttpContext.Current.Request.PhysicalPath);				
				//Console.WriteLine("UserAgent: " + HttpContext.Current.Request.UserAgent);
		


				// 开始测试
                Assert.AreEqual("POST", HttpContext.Current.Request.HttpMethod);
				Assert.AreEqual(@"c:\aaa\bb\test.dat", page.GetMappingPath1("/test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\test.dat", page.GetMappingPath1("~/test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\pages\test.dat", page.GetMappingPath1("test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\pages\dd\test.dat", page.GetMappingPath1("dd/test.dat"));

				Assert.AreEqual(@"c:\aaa\bb\test.dat", page.GetMappingPath2("/test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\test.dat", page.GetMappingPath2("~/test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\pages\test.dat", page.GetMappingPath2("test.dat"));
				Assert.AreEqual(@"c:\aaa\bb\pages\dd\test.dat", page.GetMappingPath2("dd/test.dat"));

				Assert.AreEqual("fish-li", HttpContext.Current.User.Identity.Name);
				Assert.AreEqual(true, HttpContext.Current.Request.IsAuthenticated);
				Assert.AreEqual(true, HttpContext.Current.User.Identity.IsAuthenticated);
				

				Assert.AreEqual("/a2.aspx?cc=5", HttpContext.Current.Request.UrlReferrer.PathAndQuery);
				Assert.AreEqual("/pages/abc.aspx", HttpContext.Current.Request.FilePath);
				Assert.AreEqual("/pages/abc.aspx", HttpContext.Current.Request.Path);
				Assert.AreEqual("/", HttpContext.Current.Request.ApplicationPath);
				Assert.AreEqual("~/pages/abc.aspx", HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath);
				Assert.AreEqual(contentEncoding, HttpContext.Current.Request.ContentEncoding);
				Assert.AreEqual("text", HttpContext.Current.Request.ContentType);
				Assert.AreEqual("/pages/abc.aspx?id=2&name=aa", HttpContext.Current.Request.RawUrl);
				Assert.AreEqual("POST", HttpContext.Current.Request.RequestType);
				Assert.AreEqual("http://www.test.com/pages/abc.aspx", HttpContext.Current.Request.Url.AbsoluteUri);

				Assert.AreEqual(contentEncoding.GetByteCount(formData), HttpContext.Current.Request.ContentLength);
								
				Assert.AreEqual("IE", HttpContext.Current.Request.Browser.Browser);
				Assert.AreEqual("7.0", HttpContext.Current.Request.Browser.Version);
				Assert.AreEqual("cccccccccc", HttpContext.Current.Request.Cookies["cookie1"].Value);

				Assert.AreEqual("2", page.ReadQuerySting("id"));
				Assert.AreEqual("aa", page.ReadQuerySting("name"));
				Assert.AreEqual("aa", page.ReadParams("name"));
				Assert.AreEqual("aa", page.ReadItem("name"));
				Assert.AreEqual("aa", HttpContext.Current.Request.QueryString["name"]);

				Assert.AreEqual("1", page.ReadForm("a"));
				Assert.AreEqual("2", page.ReadForm("b"));
				Assert.AreEqual("3", page.ReadForm("c"));

				Assert.AreEqual("111111111", page.ReadHeader("h1"));
				Assert.AreEqual("22222222", page.ReadHeader("h2"));

				Assert.AreEqual(appPath, page.GetAppDomainPath());
				
	
				Guid sessionData = Guid.NewGuid();
				page.WriteSession("s1", sessionData);
				Assert.AreEqual(sessionData, (Guid)page.ReadSession("s1"));
				Assert.AreEqual(sessionData, (Guid)HttpContext.Current.Session["s1"]);

				int num1 = 123;
				page.WriteApplication("key1", num1);
				Assert.AreEqual(num1, (int)page.ReadApplication("key1"));


				page.SetResponseContentType("application/octet-stream");
				Assert.AreEqual("application/octet-stream", HttpContext.Current.Response.ContentType);


				string writeText = "SQL语法分析和SQL解释实现. SQL语法分析/解释。为设计/实现SQL语法分析器提供参考";
				page.WriteResponse(writeText);
				Assert.AreEqual(writeText, context.Response.GetText());

                context.Request.SetInputStream(writeText);
                Assert.AreEqual(writeText, page.GetInputStreamString());


                context.Request.SetInputStream(writeText);
                Assert.AreEqual(writeText, context.Response.GetText());

                HttpContext.Current.Response.End();
				Console.WriteLine("============================");
				Console.WriteLine("Test OK.");
			}
		}


	}
}
