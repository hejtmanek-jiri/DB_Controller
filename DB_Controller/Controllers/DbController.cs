using DB_Controller.DbSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DB_Controller.Controllers
{
    public abstract class DbController : Controller
    {
        protected readonly GeneralDbSettings _generalDbSettings;
        protected readonly InfluxDbSettings _influxDbSettings;
        protected const string INFLUX_DB = "InfluxDb";
        protected const string TIMESCALE_DB = "TimescaleDb";

        public DbController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings)
        {
            _generalDbSettings = GeneralDbSettings.Value;
            _influxDbSettings = influxDbSettings.Value;
        }
    }
}
