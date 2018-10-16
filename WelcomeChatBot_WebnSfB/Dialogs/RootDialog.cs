using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WelcomeChatBot_WebnSfB.Models;

namespace WelcomeChatBot_WebnSfB.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public static string json = "";
        public static string convert = "";
        public static dynamic httpResponseJson = "";
        private List<string> list;
        private const string SuggestMessage = @"質問したいキーワードをスペース区切りで入力してください。
                                                入力例: リシテア 締め";
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task HelloMessage(IDialogContext context)
        {
            await context.PostAsync(SuggestMessage);

            context.Wait(MessageReceivedAsync);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            json = await CustomQnAMaker.GetResultAsync(message.Text);


            if (json != "failure")
            {
                var result = JsonConvert.DeserializeObject<QnAMakerResults>(json);

                httpResponseJson = JsonConvert.DeserializeObject(json);

                if (result.Answers[0].Answer == "No good match found in KB.")
                {
                    await context.PostAsync("質問に対する回答が見つかりませんでした。");
                    context.Done<object>(null);
                }

                else
                {
                    if (message.ChannelId == "emulator" || message.ChannelId == "webchat")
                    {
                        await ShowQuestionsWebChat(context, result);
                    }
                    else if (message.ChannelId == "skypeforbusiness")
                    {
                        await ShowQuestions(context, result);
                    }
                }

            }

        }

        private async Task ShowQuestionsWebChat(IDialogContext context, QnAMakerResults result)
        {
            int i;
            list = new List<string>();

            JObject jsonObject1 = JObject.Parse(json);

            for (i = 0; i < result.Answers.Count; i++)
            {
                JObject jsonObject2 = JObject.Parse(jsonObject1["answers"][i].ToString());

                if (jsonObject2.Last.Last.Count() == 0)
                {
                    list.Add((string)httpResponseJson.answers[i].questions[0]);
                }
                else
                {
                    list.Add("[" + (string)httpResponseJson.answers[i].metadata[0].value + "]" + " " + (string)httpResponseJson.answers[i].questions[0]);
                }
            }

            list.Add("上記のどれでもない。");

            PromptDialog.Choice(context, ShowAnswerWebChat, list, "以下から選択してください。");
        }


        private async Task ShowQuestions(IDialogContext context, QnAMakerResults result)
        {
            int i;
            string resultMessage = "以下から選択してください(番号で入力)\n";


            for (i = 0; i < result.Answers.Count; i++)
            {
                resultMessage = resultMessage + (i + 1).ToString() + ". " + result.Answers[i].Questions[0] + "\n";
            }

            resultMessage = resultMessage + (i + 1).ToString() + ". 上記のどれでもない\n";

            await context.PostAsync(resultMessage);

            context.Wait(ShowAnswer);

        }

        private async Task ShowAnswerWebChat(IDialogContext context, IAwaitable<object> result)
        {
            var selectedMenu = await result;

            var replyMessage = await CustomQnAMaker.GetResultAsync((string)selectedMenu);
            httpResponseJson = JsonConvert.DeserializeObject(replyMessage);

            if (selectedMenu.Equals("上記のどれでもない。"))
            {
                await context.PostAsync("お役に立てず申し訳ございません。。");
                await FeedbackMessage(context);
            }
            else
            {
                await context.PostAsync((string)httpResponseJson.answers[0].answer);
                await FeedbackMessage(context);
            }

        }

        private async Task ShowAnswer(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var num = await item;
            var result = JsonConvert.DeserializeObject<QnAMakerResults>(json);

            convert = await ZenkakuConvert.Convert(num.Text);

            if (Int32.Parse(convert) >= 1 && Int32.Parse(convert) <= result.Answers.Count)
            {
                await context.PostAsync(result.Answers[Int32.Parse(convert) - 1].Answer.ToString());
                await FeedbackMessage(context);
            }
            else if (Int32.Parse(convert) == result.Answers.Count + 1)
            {
                await context.PostAsync("お役に立てず申し訳ございません。。");
                await FeedbackMessage(context);
            }
            else
            {
                await ShowQuestions(context, result);
            }

        }

        private async Task FeedbackMessage(IDialogContext context)
        {
            int i;

            string[] array;
            array = new string[2] { "はい", "いいえ" };

            string resultMessage = "解決しましたか？(番号で入力)\n";

            for (i = 0; i < 2; i++)
            {
                resultMessage = resultMessage + (i + 1).ToString() + ". " + array[i] + "\n";
            }

            await context.PostAsync(resultMessage);
            context.Wait(FeedbackDialog);
        }

        private async Task FeedbackDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var feedbackMenu = await result;

            convert = await ZenkakuConvert.Convert(feedbackMenu.Text);

            if (convert == "1")
            {
                await context.PostAsync("ご利用ありがとうございました。");
               
                await HelloMessage(context);
            }
            else if (convert == "2")
            {
                await context.PostAsync("どのような回答をご希望でしたか？");
                context.Wait(InputMessage);
            }

        }

        private async Task InputMessage(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* Use below code when you want to trace logs partly */
            /* Then delete code of Line.27 in Global.assax.cs */

            //var activity = context.Activity as Microsoft.Bot.Connector.Activity;
            //Trace.TraceInformation($"{activity.Text}");

            await context.PostAsync("フィードバックありがとうございます。");

            await HelloMessage(context);

        }

    }
}