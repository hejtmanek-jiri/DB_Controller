using DB_Controller.DbSettings;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using Npgsql;
using CsvHelper;
using DB_Controller.Models;
using System.Globalization;
using System.Text;
using InfluxDB.Client.Writes;

namespace DB_Controller.Controllers
{
    public class CreateController : DbController
    {
        const int BATCH_SIZE = 5000;
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
            var options = new InfluxDBClientOptions("http://localhost:8086")
            {
                Token = _influxDbSettings.Token,
                Timeout = System.TimeSpan.FromHours(1)
            };
            using var client = InfluxDBClientFactory.Create(options);
            //using var client = new InfluxDBClient(, _influxDbSettings.Token);

            using var reader = new StreamReader("C:\\BC\\data\\data_influx.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Data>();

            var writeApi = client.GetWriteApi();

            var batch = new List<PointData>();

            foreach (var record in records)
            {
                var point = PointData.Measurement(record.Measurement)
                                     .Tag("D1", record.D1)
                                     .Tag("D2", record.D2)
                                     .Tag("D3", record.D3)
                                     .Tag("D4", record.D4)
                                     .Field("Author", record.Author)
                                     .Field("Value", record.Value)
                                     .Field("Corrected_value", record.Corrected_value)
                                     .Timestamp(record.Timestamp, WritePrecision.S);

                batch.Add(point);

                if (batch.Count >= BATCH_SIZE)
                {
                    writeApi.WritePoints(batch, _influxDbSettings.Bucket, _influxDbSettings.Org);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                try
                {
                    writeApi.WritePoints(batch, _influxDbSettings.Bucket, _influxDbSettings.Org);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            TempData["success"] = "Data successfully added!";

            return View("index");
        }

        private IActionResult CreateDataTimescaleDb()
        {
            using var streamReader = new StreamReader("C:\\BC\\data\\data_timescale.csv");

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null
            };
            var csv = new CsvReader(streamReader, config);

            var batchSize = 5000; // Příklad velikosti dávky. Můžete upravit podle potřeby.
            var batch = new List<DataTimescale>(batchSize);

            foreach (var record in csv.GetRecords<DataTimescale>())
            {
                batch.Add(record);

                if (batch.Count == batchSize)
                {
                    InsertBatch(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                InsertBatch(batch);
            }

            void InsertBatch(List<DataTimescale> batch)
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_timescaleDbSettings.ConnectionString))
                {
                    connection.Open();

                    using var transaction = connection.BeginTransaction();
                    using var command = new NpgsqlCommand();
                    command.Connection = connection;

                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO data (id, time, d1, d2, d3, d4, author, value, corrected_value) VALUES ");

                    var parameters = new List<NpgsqlParameter>();
                    for (int i = 0; i < batch.Count; i++)
                    {
                        var record = batch[i];
                        sb.AppendFormat(CultureInfo.InvariantCulture, "(@p{0}_id,  @p{0}_time, @p{0}_d1, @p{0}_d2, @p{0}_d3, @p{0}_d4, @p{0}_author, @p{0}_value, @p{0}_corrected_value ),", i);

                        parameters.Add(new NpgsqlParameter($"p{i}_id", record.Id));
                        parameters.Add(new NpgsqlParameter($"p{i}_time", record.Timestamp));
                        parameters.Add(new NpgsqlParameter($"p{i}_d1", record.D1));
                        parameters.Add(new NpgsqlParameter($"p{i}_d2", record.D2));
                        parameters.Add(new NpgsqlParameter($"p{i}_d3", record.D3));
                        parameters.Add(new NpgsqlParameter($"p{i}_d4", record.D4));
                        parameters.Add(new NpgsqlParameter($"p{i}_author", record.Author));
                        parameters.Add(new NpgsqlParameter($"p{i}_value", record.Value));
                        parameters.Add(new NpgsqlParameter($"p{i}_corrected_value", record.Corrected_value));
                    }

                    sb.Length--; // Odstranit poslední čárku
                    command.CommandText = sb.ToString();
                    command.Parameters.AddRange(parameters.ToArray());

                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }

            TempData["success"] = "Data successfully added!";

            return View("index");
        }
    }
}
