﻿namespace MTCG.Models
{
    public class ScoreboardEntry
    {
        public required string Username { get; set; }
        public string? ChosenName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public int EloPoints { get; set; }
    }
}
