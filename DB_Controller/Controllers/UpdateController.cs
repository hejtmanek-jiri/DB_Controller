using DB_Controller.DbSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using DB_Controller.Models;
using System.Globalization;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Core;
using Npgsql;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DB_Controller.Controllers
{
    public class UpdateController : DbController
    {
        const int BATCH_SIZE = 5000;
        public UpdateController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings, IOptions<TimescaleDbSettings> TimescaleDbSettings) : base(GeneralDbSettings, influxDbSettings, TimescaleDbSettings)
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
            try
            {
                //zmenit cestu na CSV kde jsou data na update
                uploadData("C:\\BC\\data\\data_influx_update.csv");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            TempData["success"] = "Data successfully updated!";

            return View("index");

        }


        private void uploadData(string path)
        {

            var options = new InfluxDBClientOptions("http://localhost:8086")
            {
                Token = _influxDbSettings.Token,
                Timeout = System.TimeSpan.FromHours(1)
            };
            using var client = InfluxDBClientFactory.Create(options);
            //using var client = new InfluxDBClient(, _influxDbSettings.Token);

            using var reader = new StreamReader(path);
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
        }

        public IActionResult UpdateDataTimescaleDb()
        {
            using var streamReader = new StreamReader("C:\\BC\\data\\data_timescale_update.csv");

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null
            };
            var csv = new CsvReader(streamReader, config);

            var batchSize = 5000;
            var batch = new List<DataTimescale>(batchSize);

            NpgsqlConnection connection = new NpgsqlConnection(_timescaleDbSettings.ConnectionString);
            connection.Open();

            foreach (var record in csv.GetRecords<DataTimescale>())
            {
                batch.Add(record);

                if (batch.Count == batchSize)
                {
                    UpdateBatch(batch, connection);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                UpdateBatch(batch, connection);
            }

            connection.Close();

            TempData["success"] = "Data successfully upadted!";

            return View("index");
        }

        private void UpdateBatch(List<DataTimescale> batch, NpgsqlConnection connection)
        {
            using var transaction = connection.BeginTransaction();
            using var command = new NpgsqlCommand();
            command.Connection = connection;

            foreach (var record in batch)
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        UPDATE data 
                        SET value = @value, corrected_value = @correctedValue
                        WHERE
                        id = @id";
                    /*
                        AND time = @timestamp 
                        AND d1 = @d1 
                        AND d2 = @d2 
                        AND d3 = @d3 
                        AND d4 = @d4
                        AND author = @author ";
                    ¨*/

                    cmd.Parameters.AddWithValue("@id", record.Id);
                    cmd.Parameters.AddWithValue("@value", record.Value);
                    cmd.Parameters.AddWithValue("@correctedValue", record.Corrected_value);
                    /*cmd.Parameters.AddWithValue("@timestamp", record.Timestamp);
                    cmd.Parameters.AddWithValue("@author", record.Author);
                    cmd.Parameters.AddWithValue("@d1", record.D1);
                    cmd.Parameters.AddWithValue("@d2", record.D2);
                    cmd.Parameters.AddWithValue("@d3", record.D3);
                    cmd.Parameters.AddWithValue("@d4", record.D4);
                    */

                    cmd.ExecuteNonQuery();
                }
            }

           // transaction.Commit();
        }
    }
}
