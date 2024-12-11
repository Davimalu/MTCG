using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models.Enums;
using MTCG.Interfaces;

namespace MTCG.Logic
{
    public class EventService : IEventService
    {
        public void LogEvent(EventType eventType, string message, Exception? ex)
        {
            string? consoleText = string.Empty;

            // Set appropriate color and text
            switch (eventType)
            {
                case EventType.Info:
                    consoleText = "INFO";
                    Console.ResetColor();
                    break;
                case EventType.Highlight:
                    consoleText = "INFO";
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case EventType.Warning:
                    consoleText = "WARNING";
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case EventType.Error:
                    consoleText = "ERROR";
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.Write($"[{consoleText}] {message}");
            if (ex != null)
            {
                Console.Write($"\n[{consoleText}] {ex.Message}");
            }
            
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
