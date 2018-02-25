using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;

namespace RA.Tests
{
	[TestFixture]
	public class CookieTest
	{
		[Test]
		public void ShouldSetCookies()
		{
			new RestAssured()
				.Given()
				.Name("Should set cookies")
				.Cookie(_singleCookie.Key, _singleCookie.Value)
				.Cookies(_multipleCookies)
				.When()
				.Get("https://httpbin.org/cookies")
				.Then()
				.TestBody("Should contain cookies", resp => TestCookieResponse(resp))
				.Assert("Should contain cookies")
				.Debug();
		}

		private bool TestCookieResponse(dynamic response)
		{
			return response.cookies.cookie1 == _singleCookie.Value &&
			       response.cookies.cookie2 == _multipleCookies["cookie2"] &&
			       response.cookies.cookie3 == _multipleCookies["cookie3"];
		}

		private KeyValuePair<string, string> _singleCookie = new KeyValuePair<string, string>("cookie1", "value1");

		private Dictionary<string, string> _multipleCookies = new Dictionary<string, string>()
		{
			{ "cookie2", "value2" },
			{ "cookie3", "value3" },
		};
	}
}
