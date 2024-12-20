using MTCG.Models.Enums;

namespace MTCG.Interfaces.Logic;

public interface IEventService
{
    void LogEvent(EventType eventType, string message, Exception? ex);
}