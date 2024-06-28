using GenHTTP.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace WebSimulateSNP42X.Tests
{
    [TestClass]
    public class BookServiceTests
    {

        [TestMethod]
        public async Task TestGetBooks()
        {
            using var runner = TestHost.Run(Project.Setup());

            using var response = await runner.GetResponseAsync("/books/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

    }

}
