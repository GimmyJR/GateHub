namespace GateHub.Dtos
{
    public class DailyReportDto
    {
        public int Hour { get; set; }
        public int Cars { get; set; }
        public decimal Revenue { get; set; }
        public int LostVehicles { get; set; } = 0;
    }


}
