using InfluxDB.Client.Core.Flux.Domain;

namespace DB_Controller.Models
{
    public class DateTimeFormViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-3);
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public string? Author { get; set; }
        public string? D1 { get; set; }
        public string? D2 { get; set; }
        public string? D3 { get; set; }
        public string? D4 { get; set; }
        public string? LogicD1 { get; set; }
        public string? LogicD2 { get; set; }
        public string? LogicD3 { get; set; }
        public string? LogicD4 { get; set; }

        public int? Count { get; set; }
        public List<FluxTable>? RecordsFlux { get; set; }
        public List<DataTimescale>? RecordsTimescale { get; set; }
        public string? Db { get; set; }
    }
}
