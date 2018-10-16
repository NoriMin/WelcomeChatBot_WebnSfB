using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Diagnostics;
using System.Linq;
using System.Web.Http.Description;
using WelcomeChatBot_WebnSfB.Dialogs;

namespace WelcomeChatBot_WebnSfB
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>

        private const string WelcomeMessage = @"こんにちは。チャットボット(仮)です。
                                                (人事、総務、工場設備、システム、＊＊＊)に関する質問に答えることが出来ます。";
        private const string SuggestMessage = @"質問したいキーワードをスペース区切りで入力してください。
                                                入力例: リシテア 締め";

        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Microsoft.Bot.Connector.Activity activity)
        {

            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:

                        //We start with the root dialog
                        await Conversation.SendAsync(activity, () => new RootDialog());


                        break;

                    case ActivityTypes.ConversationUpdate:

                        IConversationUpdateActivity update = activity;
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                        {
                            var client = scope.Resolve<IConnectorClient>();
                            if (update.MembersAdded.Any())
                            {
                                var reply = activity.CreateReply();
                                foreach (var newMember in update.MembersAdded)
                                {
                                    if (newMember.Id != activity.Recipient.Id)
                                    {
                                        //reply.Text = $"こんにちは {newMember.Name} さん!";
                                        reply.Text = WelcomeMessage;
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                        reply.Text = SuggestMessage;
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                    }

                                }
                            }
                        }

                        break;


                    case ActivityTypes.ContactRelationUpdate:

                    //Utility.savetodatabase("Start Of ActivityType.ContactRelationUpdate ", null, activity.From.Id, "Trace");

                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    case ActivityTypes.Ping:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

    }
}