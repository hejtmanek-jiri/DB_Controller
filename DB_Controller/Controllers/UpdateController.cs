using DB_Controller.DbSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DB_Controller.Controllers
{
    public class UpdateController : DbController
    {
        public UpdateController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings) : base(GeneralDbSettings, influxDbSettings)
        {
        }

        [Route("update")]
        public IActionResult UpdateData()
        {
            if (_generalDbSettings.UsedDatabase == INFLUX_DB)
            {
                return UpdateDataInfluxDb();
            }

            return UpdateDataTimescaleDb();
        }

        public IActionResult UpdateDataInfluxDb()
        {
            return Ok("Ok");

        }

        public IActionResult UpdateDataTimescaleDb()
        {
            return Problem("TimescaleDb not implemented yet", null, StatusCodes.Status501NotImplemented);
        }
    }
}
