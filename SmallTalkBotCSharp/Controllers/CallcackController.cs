using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Google.Cloud.Dialogflow.V2;
using System.Text;

namespace SmallTalkBotCSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly IVkApi _vkApi;
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Models.Updates updates)
        {
            // Проверяем, что находится в поле "type" 
            switch (updates.Type)
            {
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);
                case "message_new":
                    {
                        // Десериализация
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        string answer = DetectIntentFromTexts("smalltalkbot - atvxnh", Guid.NewGuid().ToString(), msg.Text.Split(' '));
                        // Отправим в ответ полученный от пользователя текст
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                            PeerId = msg.PeerId.Value,
                            Message = answer
                            
                        });
                        break;
                    }
            }
            // Возвращаем "ok" серверу Callback API
            return Ok("ok");
        }

        public static string DetectIntentFromTexts(string projectId,
                                                string sessionId,
                                                string[] texts,
                                                string languageCode = "ru")
        {
            var client = SessionsClient.Create();

            StringBuilder answer = new StringBuilder();

            foreach (var text in texts)
            {
                var response = client.DetectIntent(
                    session: new SessionName(projectId, sessionId),
                    queryInput: new QueryInput()
                    {
                        Text = new TextInput()
                        {
                            Text = text,
                            LanguageCode = languageCode
                        }
                    }
                );

                var queryResult = response.QueryResult;

                answer.Append(" " + queryResult.FulfillmentText);

                //Console.WriteLine($"Query text: {queryResult.QueryText}");
                //if (queryResult.Intent != null)
                //{
                //    Console.WriteLine($"Intent detected: {queryResult.Intent.DisplayName}");
                //}
                //Console.WriteLine($"Intent confidence: {queryResult.IntentDetectionConfidence}");
                //Console.WriteLine($"Fulfillment text: {queryResult.FulfillmentText}");
            }

            return answer.ToString();
        }
    }
}
