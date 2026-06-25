namespace WeatherApi.Models;

    public class WeatherRecord
    {
        public int Id { get; set; }
        public string RawJson { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
