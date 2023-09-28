using CsvHelper;
using DB_Controller.DbSettings;
using DB_Controller.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace DB_Controller.Controllers
{
    [Obsolete("This controller is deprecated and should not be used!")]
    public class InfluxdbController : DbController
    {
        public InfluxdbController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings) : base(GeneralDbSettings, influxDbSettings)
        {
        }

        // GET: InfluxdbController
        public ActionResult Index()
        {

            return View();
        }

        private void uploadData(string path)
        {
            using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

            using (var writeApi = client.GetWriteApi())
            {
                using (var reader = new StreamReader(path, System.Text.Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var record = reader.ReadLine();
                        writeApi.WriteRecord(record, WritePrecision.S, _influxDbSettings.Bucket, _influxDbSettings.Org);
                    }
                }
            }
        }

        public IActionResult WriteData()
        {
            try {
                uploadData("C:\\BC\\data\\data.csv");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

            TempData["success"] = "Data successfully added!";

            return View("index");
        }

        public IActionResult DeleteData()
        {
            var viewModel = new DateTimeFormViewModel
            {
                // Set default values or handle as per your requirement
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            TempData["success"] = "Data successfully deleted!";

            return View(viewModel);
        }

        public async Task<IActionResult> SubmitDeleteForm(DateTimeFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

                try
                {
                    var deleteApi = client.GetDeleteApi();

                    await deleteApi.Delete(viewModel.StartDate, viewModel.EndDate, "_measurement=TEST_DATA", _influxDbSettings.Bucket, _influxDbSettings.Org);
                }
                catch (Exception ex)
                {
                    return Ok(ex.Message);
                }

                // Redirect to a success page or do other post-processing
                return View("ShowData", viewModel);
            }

            // If the model state is not valid, return the same view with validation errors
            return View("Index", viewModel);
        }

        public IActionResult UpdateData()
        {
            try
            {
                uploadData("C:\\BC\\data\\updateData.csv");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

            TempData["success"] = "Data successfully updated!";

            return View("index");
        }

        public async Task<IActionResult> ShowDataOld(DateTimeFormViewModel viewModel)
        {

            if (viewModel.StartDate == DateTime.MinValue && viewModel.EndDate == DateTime.MinValue)
            {
                viewModel.StartDate = DateTime.Now.AddMonths(-3);
                viewModel.EndDate = DateTime.Now.AddDays(1);
            }

            using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

            string start = viewModel.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string end = viewModel.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string flux = "from(bucket:\""+ _influxDbSettings.Bucket + "\") |> range(start: " + start + ", stop: " + end + ") |> filter(fn: (r) => r._measurement == \"TEST_DATA\")";

            if (viewModel.Author != null && viewModel.Author != "")
            {
                flux += " |> filter(fn: (r) => r.AUTHOR == \""+ viewModel.Author + "\")";
            }
            if (viewModel.D1 != null && viewModel.D1 != "")
            {
                flux += " |> filter(fn: (r) => r.D1 == \"" + viewModel.D1 + "\")";
            }
            if (viewModel.D2 != null && viewModel.D2 != "")
            {
                flux += " |> filter(fn: (r) => r.D2 == \"" + viewModel.D2 + "\")";
            }
            if (viewModel.D3 != null && viewModel.D3 != "")
            {
                flux += " |> filter(fn: (r) => r.D3 == \"" + viewModel.D3 + "\")";
            }
            if (viewModel.D4 != null && viewModel.D4 != "")
            {
                flux += " |> filter(fn: (r) => r.D4 == \"" + viewModel.D4 + "\")";
            }
            //var flux = "from(bucket:\"Test\") |> range(start: " + start + ")";

            var fluxTables = await client.GetQueryApi().QueryAsync(flux, _influxDbSettings.Org);

            var records = fluxTables.ToList();
            ViewBag.records = records;

            return View(viewModel);
        }


        public async Task<IActionResult> ShowData(DateTimeFormViewModel viewModel)
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
                    filter +=  "r.AUTHOR == \"" + viewModel.Author + "\" ";
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

                flux +=  filter + ")";

            }

            var fluxTables = await client.GetQueryApi().QueryAsync(flux, _influxDbSettings.Org);

            var records = fluxTables.ToList();
            ViewBag.records = records;

            return View(viewModel);
        }

    }

    
}
