using CsvHelper;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DB_Controller.Controllers
{
    public class InfluxdbController : Controller
    {
        // GET: InfluxdbController
        public ActionResult Index()
        {

            return View();
        }

        public IActionResult WriteData()
        {
            var token = "02ETE6C2iCcPGs2wmXZBEsPWTIfj0IuWSKru7diR5cdpKWbwQcZBmzd-zhcPhtZNjRzUX6CA0l2AhwS1S1QtFw==";
            const string bucket = "Test";
            const string org = "Unicorn University";

            using var client = new InfluxDBClient("http://localhost:8086", token);

            //const string data = "123456,host=host1 used_percent=23.43234543";
            try { 
                using (var writeApi = client.GetWriteApi())
                {
                    using (var reader = new StreamReader("C:\\BC\\data\\data.csv"))
                    {
                        while (!reader.EndOfStream)
                        {
                            var record = reader.ReadLine();
                            writeApi.WriteRecord(record, WritePrecision.Us, bucket, org);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }


            return Ok("OK");
        }

    }
}
