using CsvHelper;
using DB_Controller.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DB_Controller.Controllers
{
    public class InfluxdbController : Controller
    {
        private const string TOKEN = "3IA4EAnCUMBGqEFPUwXSsiEgamD2xickHBmjk8l5UiqU8OBRnsChgaBVhGq0plueXzn4FhsUfkzAg9ghXLyWLA==";
        private const string BUCKET = "TEST_DATA";
        private const string ORG = "TEST_ORG";
        // GET: InfluxdbController
        public ActionResult Index()
        {

            return View();
        }

        private void uploadData(string path)
        {
            using var client = new InfluxDBClient("http://localhost:8086", TOKEN);

            using (var writeApi = client.GetWriteApi())
            {
                using (var reader = new StreamReader(path, System.Text.Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var record = reader.ReadLine();
                        writeApi.WriteRecord(record, WritePrecision.S, BUCKET, ORG);
                    }
                }
            }
        }

        public IActionResult WriteData()
        {
            //var token = "02ETE6C2iCcPGs2wmXZBEsPWTIfj0IuWSKru7diR5cdpKWbwQcZBmzd-zhcPhtZNjRzUX6CA0l2AhwS1S1QtFw==";
            try {
                uploadData("C:\\BC\\data\\data.csv");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }


            return Ok("OK");
        }

        public IActionResult DeleteData()
        {
            var viewModel = new DateTimeFormViewModel
            {
                // Set default values or handle as per your requirement
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> SubmitDeleteForm(DateTimeFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using var client = new InfluxDBClient("http://localhost:8086", TOKEN);

                string selectString = "";

                try
                {
                    var deleteApi = client.GetDeleteApi();

                    /*DeletePredicateRequest dpr = new DeletePredicateRequest
                    {
                        Start = viewModel.StartDate,
                        Stop = viewModel.EndDate,
                        Predicate = selectString
                    };*/

                    await deleteApi.Delete(viewModel.StartDate, viewModel.StartDate, "_measurement=TEST_DATA", BUCKET, ORG);
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


            return Ok("OK");
        }

        public async Task<IActionResult> ShowData(DateTimeFormViewModel viewModel)
        {

            if (viewModel.StartDate == DateTime.MinValue && viewModel.EndDate == DateTime.MinValue)
            {
                viewModel.StartDate = DateTime.Now.AddMonths(-3);
                viewModel.EndDate = DateTime.Now.AddDays(1);
            }

            using var client = new InfluxDBClient("http://localhost:8086", TOKEN);

            string start = viewModel.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string end = viewModel.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string flux = "from(bucket:\""+ BUCKET + "\") |> range(start: " + start + ", stop: " + end + ") |> filter(fn: (r) => r._measurement == \"TEST_DATA\")";

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

            var fluxTables = await client.GetQueryApi().QueryAsync(flux, ORG);

            var records = fluxTables.ToList();
            ViewBag.records = records;

            return View(viewModel);
        }

    }

    
}
