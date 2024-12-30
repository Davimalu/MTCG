using MTCG.Interfaces.Logic;
using OpenAI.Chat;
using System.Text.Json;
using MTCG.Models.Enums;

namespace MTCG.Logic
{
    public class AIService : IAIService
    {
        #region Singleton
        private static AIService? _instance;

        public static AIService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AIService();
                }

                return _instance;
            }
        }
        #endregion

        private readonly IEventService _eventService = new EventService();


        public string? GetListOfCards(string theme, string apiKey)
        {
            _eventService.LogEvent(EventType.Info, "Authorize with OpenAI API...", null);
            ChatClient client = new ChatClient(model: "gpt-4o", apiKey: apiKey);
            _eventService.LogEvent(EventType.Highlight, "Authorized with OpenAI API", null);

            string systemPrompt = $"You are a game designer tasked with creating cards for a Trading Card Game. Players can collect these cards and use them in battles against each other. Each card consists of a unique ID, a name, and a damage value. Here is an example of a fully defined card: {{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}} The ID should follow the format shown and must be unique. Damage values are floating-point numbers and should range between 5 and 150. The name can be freely chosen, but the following rules apply: There are two types of cards: Monster Cards and Spell Cards. Spell Cards are any cards whose names include the word \"Spell,\" such as \"WaterSpell\" or \"FireSpell.\" All cards whose names do not include \"Spell\" are automatically Monster Cards. Both types of cards can have different element types: Fire, Water, or Normal. A card whose name includes \"Water\" has the element type Water. A card whose name includes \"Fire\" has the element type Fire. A card whose name includes neither string has the element type Normal. Note that a card cannot be both a monster and spell card and can only have a single element type.  Create five such cards with names from the theme: {theme}. Make sure the cards are balanced, and none are particularly strong or weak.";

            List<ChatMessage> messages =
            [
                new UserChatMessage(systemPrompt)
            ];

            // top-level must be `object`, `array` as top-level is not supported by the API
            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "Cards",
                    jsonSchema: BinaryData.FromBytes("""
                                                     {
                                                         "type": "object",
                                                         "properties": {
                                                             "Cards": {
                                                                 "type": "array",
                                                                 "items": {
                                                                     "type": "object",
                                                                     "properties": {
                                                                         "Id": { "type": "string" },
                                                                         "Name": { "type": "string" },
                                                                         "Damage": { "type": "number" }
                                                                     },
                                                                     "required": ["Id", "Name", "Damage"],
                                                                     "additionalProperties": false
                                                                 }
                                                             }
                                                         },
                                                         "required": ["Cards"],
                                                         "additionalProperties": false
                                                     }
                                                     """u8.ToArray()),
                    jsonSchemaIsStrict: true)
            };

            _eventService.LogEvent(EventType.Highlight, "Waiting for response from OpenAI API...", null);
            ChatCompletion completion = client.CompleteChat(messages, options);

            _eventService.LogEvent(EventType.Highlight, $"Received response from ChatGPT 4o:", null);
            _eventService.LogEvent(EventType.Info, completion.Content[0].Text, null);

            return completion.Content[0].Text;
        }
    }
}
