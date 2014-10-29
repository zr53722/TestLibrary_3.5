using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Specialized;

namespace TestLibrary
{
	public sealed class MockHttpRequest
	{
		private HttpRequest _request;
		private NameValueCollection _headers;


		internal MockHttpRequest(HttpRequest request)
        {
			_request = request;
			

			//为了解决Response.Redirect()方法。
			request.Browser = new MockHttpBrowserCapabilities() as HttpBrowserCapabilities;

			request.ContentEncoding = Encoding.UTF8;
			request.ContentType = "text";
        }

		public MockHttpBrowserCapabilities Browser
		{
			get { return (MockHttpBrowserCapabilities)_request.Browser; }
		}


		public Encoding ContentEncoding
		{
			set { _request.ContentEncoding = value; }
		}

		public string ContentType
		{
			set { _request.ContentType = value; }
		}

		public string RequestType
		{
			set
            {
                if (string.Equals(value, "post", StringComparison.OrdinalIgnoreCase))
                {
                    _request.GetType().GetInstanceField("_httpMethod").SetValue(_request, "POST");
                }
                else
                {
                    _request.GetType().GetInstanceField("_httpMethod").SetValue(_request, "GET");
                    _request.RequestType = value;
                }
            }
		}


		/// <summary>
		/// 给 HttpRequest.Form 集合添加数据。
		/// </summary>
		/// <param name="form"></param>
		public void SetForm(string formData)
		{
			if( string.IsNullOrEmpty(formData) )
				return;

			// internal HttpValueCollection(string str, bool readOnly, bool urlencoded, Encoding encoding)
			object instance = WebHelper.CreateInstance("System.Web.HttpValueCollection", formData, true, true, Encoding.Default);
			_request.GetType().GetInstanceField("_form").SetValue(_request, instance);


			int length = _request.ContentEncoding.GetByteCount(formData);
			_request.GetType().GetInstanceField("_contentLength").SetValue(_request, length);
		}


   

		/// <summary>
		/// 给 HttpRequest.Cookies 集合添加数据。
		/// </summary>
		/// <param name="cookie"></param>
		public void AddCookie(HttpCookie cookie)
		{
			_request.Cookies.Add(cookie);
		}

		

		/// <summary>
		/// 给 HttpRequest.Headers 集合添加数据。
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddHeader(string name, string value)
		{
			if( _headers == null ) {
				//_headers = new NameValueCollection();

				Type t = typeof(HttpContext).Assembly.GetType("System.Web.HttpHeaderCollection");
				ConstructorInfo ctor = t.GetConstructor( BindingFlags.NonPublic| BindingFlags.Instance, null, 
					new Type[] {typeof(HttpWorkerRequest), typeof(HttpRequest), typeof(int)}, null);

				_headers = ctor.Invoke(new object[]{null, null, 20}) as NameValueCollection;

				_request.GetType().GetInstanceField("_headers").SetValue(_request, _headers);
			}


			//_headers.Add(name, value);
			_headers.GetType().InvokeMember("SynchronizeHeader",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null,
				_headers, new object[] { name, value });
		}

		public string UrlReferrer
		{
			set
			{
				if( string.IsNullOrEmpty(value) )
					throw new ArgumentNullException("value");

				Uri uri = new Uri(WebContext.CheckUrl(value));

				_request.GetType().GetInstanceField("_referrer").SetValue(_request, uri);
			}
		}




		//public Stream InputStream
		//{
		//    set
		//    {
		//        if( value.CanRead == false )
		//            throw new ArgumentException("指定的流不可读。");

		//        _request.GetType().GetInstanceField("_inputStream").SetValue(_request, value);
		//        //_request.GetType().GetInstanceField("_contentLength").SetValue(_request, value.Length);
		//    }
		//}


		public void SetInputStream(string text)
		{
			byte[] buffer = _request.ContentEncoding.GetBytes(text);

			SetInputStream(buffer);
		}

		public void SetInputStream(byte[] buffer)
		{
			if( buffer == null )
				throw new ArgumentNullException("buffer");

			// 构造HttpRawUploadedContent实例，并填充数据。
			Type httpRawUploadedContentType = typeof(HttpContext).Assembly.GetType("System.Web.HttpRawUploadedContent");
			if( httpRawUploadedContentType == null )
				throw new NotSupportedException();


			ConstructorInfo httpRawUploadedContentCtor = httpRawUploadedContentType.GetSpecificCtor( typeof(int), typeof(int) );
			if( httpRawUploadedContentCtor == null )
				throw new NotSupportedException();


			object httpRawUploadedContentObject = httpRawUploadedContentCtor.Invoke(new object[] { buffer.Length, buffer.Length });

			MethodInfo addBytesMethod = httpRawUploadedContentObject.GetType().GetMethod("AddBytes", BindingFlags.Instance | BindingFlags.NonPublic);
			addBytesMethod.Invoke(httpRawUploadedContentObject, new object[] { buffer, 0, buffer.Length });

			httpRawUploadedContentObject.GetType().InvokeMember("DoneAddingBytes",
				 BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null,
				 httpRawUploadedContentObject, null);


			// 构造HttpInputStream实例
			Type httpInputStremType = typeof(HttpContext).Assembly.GetType("System.Web.HttpInputStream");
			if( httpInputStremType == null )
				throw new NotSupportedException();


			ConstructorInfo httpInputStremCtor = httpInputStremType.GetSpecificCtor(httpRawUploadedContentType, typeof(int), typeof(int) );
			if( httpRawUploadedContentCtor == null )
				throw new NotSupportedException();

			object httpInputStremObject = httpInputStremCtor.Invoke(new object[] { httpRawUploadedContentObject, 0, buffer.Length });


			// 替换Request.InputStream对象
			_request.GetType().GetInstanceField("_inputStream").SetValue(_request, httpInputStremObject);


            //start
            //HttpWriter hw = Activator.CreateInstance(typeof(HttpWriter),
            //     BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
            //     new object[] { HttpContext.Current.Response }, CultureInfo.CurrentCulture) as HttpWriter;
          
            //Stream hrs = Activator.CreateInstance(typeof(HttpWriter).Assembly.GetType("System.Web.HttpResponseStream"),
            //     BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
            //     new object[] { hw }, CultureInfo.CurrentCulture) as Stream;

            //hw.GetType().GetInstanceField("_stream").SetValue(hw, hrs);
            //hw.WriteBytes(buffer, 0, buffer.Length);
            //typeof(System.Web.HttpResponse).GetInstanceField("_httpWriter").SetValue(HttpContext.Current.Response, hw);
            //typeof(System.Web.HttpResponse).GetInstanceField("_writer").SetValue(HttpContext.Current.Response, hw);
            //end
		}

	}
}
