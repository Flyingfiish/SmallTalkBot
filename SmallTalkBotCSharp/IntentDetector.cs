using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Grpc.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmallTalkBotCSharp
{
    public class IntentDetector
    {
        public static GoogleCredential Creds = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), @"SmallTalkBot.json"));

        public static string DetectIntent(string text)
        {
            var query = new QueryInput
            {
                Text = new TextInput
                {
                    Text = text,
                    LanguageCode = "ru"
                }
            };

            var sessionId = Guid.NewGuid().ToString();
            var agent = "smalltalkbot-atvxnh";
            var creds = IntentDetector.Creds;
            var channel = new Grpc.Core.Channel(SessionsClient.DefaultEndpoint.Host,
                          creds.ToChannelCredentials());

            var client = SessionsClient.Create(channel);

            var dialogFlow = client.DetectIntent(
                new SessionName(agent, sessionId),
                query
            );
            channel.ShutdownAsync();
            return dialogFlow.QueryResult.FulfillmentText;
        }
    }
}

