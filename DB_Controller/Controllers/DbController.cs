using DB_Controller.DbSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DB_Controller.Controllers
{
    [DB_Controller.Filters.ExceptionFilter]
    public abstract class DbController : Controller
    {
        protected readonly GeneralDbSettings _generalDbSettings;
        protected readonly InfluxDbSettings _influxDbSettings;
        protected readonly TimescaleDbSettings _timescaleDbSettings;
        public const string INFLUX_DB = "InfluxDb";
        public const string TIMESCALE_DB = "TimescaleDb";

        public DbController(IOptions<GeneralDbSettings> GeneralDbSettings, IOptions<InfluxDbSettings> influxDbSettings, IOptions<TimescaleDbSettings> TimescaleDbSettings)
        {
            _generalDbSettings = GeneralDbSettings.Value;
            _influxDbSettings = influxDbSettings.Value;
            _timescaleDbSettings = TimescaleDbSettings.Value;
        }
    }
}
