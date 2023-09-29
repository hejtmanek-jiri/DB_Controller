using DB_Controller.DbSettings;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;

namespace DB_Controller.Controllers
{
    public class CreateController : DbController
    {
        public CreateController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings, IOptions<TimescaleDbSettings> TimescaleDbSettings) : base(GeneralDbSettings, influxDbSettings, TimescaleDbSettings)
        {
        }

        [Route("create")]
        public IActionResult CreateData()
        {
            if (_generalDbSettings.UsedDatabase == INFLUX_DB)
            {
                    return CreateDataInfluxDb();
            }
            
            return CreateDataTimescaleDb();
        }

        private IActionResult CreateDataInfluxDb()
        {
            using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

            using (var writeApi = client.GetWriteApi())
            {
                using (var reader = new StreamReader("C:\\BC\\data\\data.csv", System.Text.Encoding.UTF8))
                {
                    try 
                    { 
                        while (!reader.EndOfStream)
                        {
                            var record = reader.ReadLine();
                            writeApi.WriteRecord(record, WritePrecision.S, _influxDbSettings.Bucket, _influxDbSettings.Org);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Problem(ex.Message, null, StatusCodes.Status400BadRequest);
                    }
                }
            }

            TempData["success"] = "Data successfully added!";

            return View("index");
        }

        private IActionResult CreateDataTimescaleDb()
        {
            return Problem("TimescaleDb not implemented yet", null, StatusCodes.Status501NotImplemented);
        }
    }
}
