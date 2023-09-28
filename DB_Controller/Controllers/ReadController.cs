using DB_Controller.DbSettings;
using DB_Controller.Models;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DB_Controller.Controllers
{
    public class ReadController : DbController
    {
        public ReadController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings) : base(GeneralDbSettings, influxDbSettings)
        {
        }

        [Route("read")]
        public async Task<IActionResult> ReadData(DateTimeFormViewModel viewModel)
        {
            if (_generalDbSettings.UsedDatabase == INFLUX_DB)
            {
                return await ReadDataInfluxDb(viewModel);
            }
            
            return ReadDataTimescaleDb(viewModel);

        }

        private async Task<IActionResult> ReadDataInfluxDb(DateTimeFormViewModel viewModel)
        {
            if (viewModel.StartDate == DateTime.MinValue && viewModel.EndDate == DateTime.MinValue)
            {
                viewModel.StartDate = DateTime.Now.AddMonths(-3);
                viewModel.EndDate = DateTime.Now.AddDays(1);
            }

            using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

            string start = viewModel.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string end = viewModel.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string flux = "from(bucket:\"" + _influxDbSettings.Bucket + "\") |> range(start: " + start + ", stop: " + end + ") |> filter(fn: (r) => r._measurement == \"TEST_DATA\")";

            if (viewModel.Author != null || viewModel.D1 != null || viewModel.D2 != null || viewModel.D3 != null || viewModel.D4 != null)
            {
                string filter = " |> filter(fn: (r) => ";
                Boolean isFirst = true;


                if (viewModel.Author != null && viewModel.Author != "")
                {
                    isFirst = false;
                    filter += "r.AUTHOR == \"" + viewModel.Author + "\" ";
                }
                if (viewModel.D1 != null && viewModel.D1 != "")
                {
                    if (!isFirst)
                    {
                        filter += viewModel.LogicD1;

                    }
                    isFirst = false;
                    filter += " r.D1 == \"" + viewModel.D1 + "\" ";
                }
                if (viewModel.D2 != null && viewModel.D2 != "")
                {
                    if (!isFirst)
                    {
                        filter += viewModel.LogicD2;

                    }
                    isFirst = false;
                    filter += "  r.D2 == \"" + viewModel.D2 + "\" ";
                }
                if (viewModel.D3 != null && viewModel.D3 != "")
                {
                    if (!isFirst)
                    {
                        filter += viewModel.LogicD3;

                    }
                    isFirst = false;
                    filter += "  r.D3 == \"" + viewModel.D3 + "\" ";
                }
                if (viewModel.D4 != null && viewModel.D4 != "")
                {
                    if (!isFirst)
                    {
                        filter += viewModel.LogicD4;

                    }
                    isFirst = false;
                    filter += " r.D4 == \"" + viewModel.D4 + "\" ";
                }

                flux += filter + ")";

            }

            try
            {
                var fluxTables = await client.GetQueryApi().QueryAsync(flux, _influxDbSettings.Org);
            
                var records = fluxTables.ToList();
                ViewBag.records = records;
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            return View(viewModel);
        }

        private IActionResult ReadDataTimescaleDb(DateTimeFormViewModel viewModel)
        {
            return Problem("TimescaleDb not implemented yet", null, StatusCodes.Status501NotImplemented);
        }
    }
}
