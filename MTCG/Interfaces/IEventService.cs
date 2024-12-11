using MTCG.Models.Enums;

namespace MTCG.Interfaces;

public interface IEventService
{
    void LogEvent(EventType eventType, string message, Exception? ex);
}