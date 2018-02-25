using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RA.Tests
{
	[TestFixture]
	public class TimeoutTest
	{
		[Test]
		public void ExceededTimeoutShouldFail()
		{
			Assert.Throws<TaskCanceledException>(() =>
			{
				new RestAssured()
					.Given()
					.Name("Exceeded Timeout Call")
					.Timeout(1000)
					.When()
					.Get("http://httpbin.org/delay/3")
					.Then()
					.Debug();
			});
		}

		[Test]
		public void EnoughTimeoutShouldSucceed()
		{
			new RestAssured()
				.Given()
				.Name("Enough Timeout Call")
				.Timeout(5000)
				.When()
				.Get("http://httpbin.org/delay/3")
				.Then()
				.Debug();
		}

		[Test]
		public void WithoutTimeoutShouldSucceed()
		{
			new RestAssured()
				.Given()
				.Name("Without Timeout Call")
				.When()
				.Get("http://httpbin.org/delay/3")
				.Then()
				.Debug();
		}
	}
}
