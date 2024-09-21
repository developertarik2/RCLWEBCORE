using Microsoft.AspNetCore.Mvc;
using RCLWEBCORE.Controllers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RCLWEBCORE.Areas.Branch.Controllers
{
    [Area("Branch")]
    [Route("api/[controller]")]
    [ApiController]
    public class Values2Controller : ControllerBase
    {
        // GET: api/<Values2Controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<Values2Controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Values2Controller>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Values2Controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Values2Controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpPost]
        [Route("add")]
        public ActionResult Addwew(Insert insert)
        {
            return Ok("value1");
        }
    }
}
