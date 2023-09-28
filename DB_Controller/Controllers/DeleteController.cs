using DB_Controller.DbSettings;
using DB_Controller.Models;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DB_Controller.Controllers
{
    public class DeleteController : DbController
    {
        public DeleteController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings) : base(GeneralDbSettings, influxDbSettings)
        {
        }

        [Route("delete")]
        public IActionResult Index()
        {
            var viewModel = new DateTimeFormViewModel
            {
                // Set default values or handle as per your requirement
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            return View(viewModel);
        }

        
        public async Task<IActionResult> DeleteDataSubmit(DateTimeFormViewModel viewModel)
        {
            if (_generalDbSettings.UsedDatabase == INFLUX_DB)
            {
                return await DeleteDataInfluxDb(viewModel);
            }

            return DeleteDataTimescaleDb();
        }

        public async Task<IActionResult> DeleteDataInfluxDb(DateTimeFormViewModel viewModel)
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
                return View("Index", viewModel);
            }

            // If the model state is not valid, return the same view with validation errors
            return View("Index", viewModel);
        }

        public IActionResult DeleteDataTimescaleDb()
        {
            return Problem("TimescaleDb not implemented yet", null, StatusCodes.Status501NotImplemented);
        }
    }
}
