using DB_Controller.DbSettings;
using DB_Controller.Models;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DB_Controller.Controllers
{
    public class ReadController : DbController
    {
        public ReadController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings, IOptions<TimescaleDbSettings> TimescaleDbSettings) : base(GeneralDbSettings, influxDbSettings, TimescaleDbSettings)
        {
        }

        [Route("read")]
        public async Task<IActionResult> ReadData(DateTimeFormViewModel viewModel)
        
        {
            if (_generalDbSettings.UsedDatabase == INFLUX_DB)
            {
                return await ReadDataInfluxDb(viewModel);
            }
            
            return await ReadDataTimescaleDb(viewModel);

        }

        private async Task<IActionResult> ReadDataInfluxDb(DateTimeFormViewModel viewModel)
        {
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
                ViewBag.db = INFLUX_DB;
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            return View(viewModel);
        }

        private async Task<IActionResult> ReadDataTimescaleDb(DateTimeFormViewModel viewModel)
        {
            string sql = "SELECT * FROM data ";
            sql += "WHERE time >= '" + viewModel.StartDate + "' AND time <= '" + viewModel.EndDate + "' ";

            if (viewModel.Author != null || viewModel.D1 != null || viewModel.D2 != null || viewModel.D3 != null || viewModel.D4 != null)
            {
                

                if (viewModel.Author != null && viewModel.Author != "")
                {
                    sql += "AND AUTHOR = '" + viewModel.Author +  "' ";
                }
                if (viewModel.D1 != null && viewModel.D1 != "")
                {
                    sql += viewModel.LogicD1 + " ";
                    sql += "D1 = '" + viewModel.D1 + "' ";
                }
                if (viewModel.D2 != null && viewModel.D2 != "")
                {
                    sql += viewModel.LogicD2 + " ";
                    sql += "D2 = '" + viewModel.D2 + "' ";
                }
                if (viewModel.D3 != null && viewModel.D3 != "")
                {
                    sql += viewModel.LogicD3 + " ";
                    sql += "D3 = '" + viewModel.D3 + "' ";
                }
                if (viewModel.D4 != null && viewModel.D4 != "")
                {
                    sql += viewModel.LogicD4 + " ";
                    sql += "D4 = '" + viewModel.D4 + "' ";
                }

                sql += " ORDER BY time ASC";

            }

            var resultList = new List<DataTimescale>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_timescaleDbSettings.ConnectionString))
            {
                connection.Open();

                // Vytvoření a provedení SQL dotazu
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resultList.Add(new DataTimescale
                        {
                            D1 = reader.IsDBNull(reader.GetOrdinal("d1")) ? null : reader.GetString(reader.GetOrdinal("d1")),
                            D2 = reader.IsDBNull(reader.GetOrdinal("d2")) ? null : reader.GetString(reader.GetOrdinal("d2")),
                            D3 = reader.IsDBNull(reader.GetOrdinal("d3")) ? null : reader.GetString(reader.GetOrdinal("d3")),
                            D4 = reader.IsDBNull(reader.GetOrdinal("d4")) ? null : reader.GetString(reader.GetOrdinal("d4")),
                            Author = reader.IsDBNull(reader.GetOrdinal("author")) ? null : reader.GetString(reader.GetOrdinal("author")),
                            Value = reader.GetDouble(reader.GetOrdinal("value")),
                            Corrected_value = reader.GetDouble(reader.GetOrdinal("corrected_value")),
                            Timestamp = reader.GetDateTime(reader.GetOrdinal("time"))
                        });
                    }
                }

                ViewBag.records = resultList;
                ViewBag.db = TIMESCALE_DB;
                TempData["success"] = "Data loaded!";
            }

            //TODO změnit na View(viewModel)
            return View(viewModel);
        }
    }
}
