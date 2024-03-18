namespace DB_Controller.Models
{
    public class DataTimescale
    {
        public long Id { get; set; }
        public string D1 { get; set; }

        public string D2 { get; set; }

        public string D3 { get; set; }

        public string D4 { get; set; }

        public string Author { get; set; }

        public double Value { get; set; }

        public double Corrected_value { get; set; }

        public new DateTime Timestamp { get; set;} 
    }
}
