
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;


namespace Microsoft.BotBuilderSamples.Dialogs
{
    // Dialog Class for Extracurricular Dialog 
    public class ExtracurricularDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;


        public ExtracurricularDialog(ConversationRecognizer luisRecognizer, ILogger<ExtracurricularDialog> logger, CampusDialog campusDialog)
            : base(nameof(ExtracurricularDialog))

        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(campusDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                MoveConvoAsync,
                CampusAsync,

            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

    // Begin dialog flow 
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            //if the user has said they do an activity talk about that rather than asking again
            var userDetails = new UserProfile()
            {
                Name = luisResult.Entities.UserName,
                Activity = luisResult.Entities.Extracurricular,
            };
            var messageText = $"";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };

            if (!string.IsNullOrEmpty(userDetails.Activity.First()))
            {
                messageText = $"Great idea! Are you a part of a team or a club for {userDetails.Activity.FirstOrDefault()}?";
                elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
            }
            else
            {
                 messageText = $"Great! So what kinda things do you like doing in your spare time on campus?";
                elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
            }


            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }


        private async Task<DialogTurnResult> GetInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                Lecturer = luisResult.Entities.Lecturer,
                Opinion = luisResult.Entities.Opinion,
            };

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                string[] stringPos;
                stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
                string[] stringNeg;
                stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

                var messageText = $"Are you sure you want to end our conversation?";
                var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
                await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
                if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            var luisResult2 = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

          
            
            if(stringPos.Any(luisResult2.Text.ToLower().Contains)){
                ConversationData.PromptedUserForName = true;
               await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("It was great talking to you! Enjoy the rest of your day!", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            if(stringNeg.Any(luisResult2.Text.ToLower().Contains)){
            var messageTextNeg = $"Great! Let's continue our conversation.";
            var elsePromptMessageNeg = new PromptOptions { Prompt = MessageFactory.Text(messageTextNeg, messageTextNeg, InputHints.ExpectingInput) };
            await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNeg, cancellationToken);
            }
    }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular))
            {
                var messageText = $"Wow that's great!";
                var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
            }
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.None)){
                var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                 var elsePromptMessageNone=  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNone, cancellationToken);
            }

            var messageText2 = $"Is there anything else you'd like to talk about in relation to university?";
            var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
        }

        private async Task<DialogTurnResult> MoveConvoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string[] stringPos;
            stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
            string[] stringNeg;
            stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            if (stringNeg.Any(luisResult.Text.ToLower().Contains))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("It was great talking to you! Enjoy the rest of your day!", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            if(stringPos.Any(luisResult.Text.ToLower().Contains)){
            return await stepContext.NextAsync();
            }
            var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                 var elsePromptMessageNone=  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNone, cancellationToken);
            
        }

    private async Task<DialogTurnResult> CampusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             // Transition to Campus Dialog
            return await stepContext.BeginDialogAsync(nameof(CampusDialog));

        }


    }
}