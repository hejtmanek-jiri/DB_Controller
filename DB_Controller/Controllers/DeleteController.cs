using DB_Controller.DbSettings;
using DB_Controller.Models;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Cryptography;

namespace DB_Controller.Controllers
{
    public class DeleteController : DbController
    {
        public DeleteController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings, IOptions<TimescaleDbSettings> TimescaleDbSettings) : base(GeneralDbSettings, influxDbSettings, TimescaleDbSettings)
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

            return DeleteDataTimescaleDb(viewModel);
        }

        public async Task<IActionResult> DeleteDataInfluxDb(DateTimeFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var options = new InfluxDBClientOptions("http://localhost:8086")
                {
                    Token = _influxDbSettings.Token,
                    Timeout = System.TimeSpan.FromHours(1)
                };
                using var client = new InfluxDBClient(options);

                try
                {
                    var deleteApi = client.GetDeleteApi();

                    Task.WaitAll(deleteApi.Delete(viewModel.StartDate, viewModel.EndDate, "_measurement=TEST_DATA", _influxDbSettings.Bucket, _influxDbSettings.Org));
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

        public IActionResult DeleteDataTimescaleDb(DateTimeFormViewModel viewModel)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_timescaleDbSettings.ConnectionString))
            {
                connection.Open();

                // Vytvoření a provedení SQL dotazu
                using (var command = new NpgsqlCommand())
                {
                    command.CommandTimeout = (int)TimeSpan.FromMinutes(60).TotalSeconds;
                    command.Connection = connection;

                    command.CommandText = "DELETE FROM data WHERE time >= @StartDate AND time <= @EndDate";

                    command.Parameters.AddWithValue("@StartDate", viewModel.StartDate);
                    command.Parameters.AddWithValue("@EndDate", viewModel.EndDate);

                    try
                    {
                        command.ExecuteNonQuery();
                        TempData["success"] = "Data successfully deleted!";
                    }
                    catch (Exception ex)
                    {
                        return Ok(ex.Message);
                    }
                }
            }

            return View("Index", viewModel);
        }
    }
}
