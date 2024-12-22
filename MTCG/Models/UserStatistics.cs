namespace MTCG.Models
{
    public class UserStatistics
    {
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Ties { get; set; } = 0;
        public int EloPoints { get; set; } = 100;
    }
}
