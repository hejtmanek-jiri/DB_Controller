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
            try { 
                //zmenit cestu na CSV kde jsou data na update
                uploadData("C:\\BC\\data\\data.csv");
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
            using var client = new InfluxDBClient("http://localhost:8086", _influxDbSettings.Token);

            using var reader = new StreamReader("C:\\BC\\data\\data.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Data>();

            var writeApi = client.GetWriteApiAsync();

            var batch = new List<PointData>();

            foreach (var record in records)
            {
                var point = PointData.Measurement(record.Measurement)
                                     .Tag("D1", record.D1)
                                     .Tag("D2", record.D2)
                                     .Tag("D3", record.D3)
                                     .Tag("D4", record.D4)
                                     .Tag("Author", record.Author)
                                     .Field("Value", record.Value)
                                     .Field("Corrected_value", record.Corrected_value)
                                     .Timestamp(record.Timestamp, WritePrecision.S);
                
                batch.Add(point);

                if (batch.Count >= BATCH_SIZE)
                {
                    writeApi.WritePointsAsync(batch, _influxDbSettings.Bucket, _influxDbSettings.Org);
                    batch.Clear();
                }

               
            }
            
            if (batch.Count > 0)
            {
                try { 
                    writeApi.WritePointsAsync(batch, _influxDbSettings.Bucket, _influxDbSettings.Org);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            

        }

        public IActionResult UpdateDataTimescaleDb()
        {
            return Problem("TimescaleDb not implemented yet", null, StatusCodes.Status501NotImplemented);
        }
    }
}
