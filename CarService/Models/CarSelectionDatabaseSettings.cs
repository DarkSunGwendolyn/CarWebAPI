namespace CarWebAPI.Models
{
    public class CarSelectionDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CarsCollectionName { get; set; } = null!;
    }
}
