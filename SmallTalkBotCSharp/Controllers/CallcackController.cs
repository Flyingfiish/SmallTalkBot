using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using System.Text;
using ApiAi;
using ApiAi.Models;
using System.Linq;

namespace SmallTalkBotCSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private const string
            ClientAccessToken = "e2c282551f7a4f90858c2aa523c7b6cd",
            DeveloperAccessToken = "ce225703b1334a35817d8aa0de7e009c",
            ExampleEntityId = "your_exists_entity_id";

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
                        string answer = GetAnswer(msg.Text);
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

        public static string GetAnswer(string text)
        {
            var answer = new StringBuilder();

            var result = QueryService.SendRequest(new ConfigModel { AccesTokenClient = ClientAccessToken }, text);

            //foreach (var message in result.Messages)
            //{
            //    answer.Append(message.Text);
            //}

            return result.Messages.FirstOrDefault().Text;
        }


    }
}
