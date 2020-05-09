using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Cycler.Controllers
{
    [Route("/.well-known/acme-challenge")]
    public class SSLController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        public SSLController(IHostingEnvironment environment) {
            _hostingEnvironment = environment;
        }

        [HttpGet]
        [Route("q27Pgeo6vV4dgtS4KxF-QYbOXXX0anj22RSpFWh-NSw")]
        public IActionResult Index() {
            return Content(System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath,".well-known/acme-challenge/q27Pgeo6vV4dgtS4KxF-QYbOXXX0anj22RSpFWh-NSw")), "text/plain");
        }
    }
}