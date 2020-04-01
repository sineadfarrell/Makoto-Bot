
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class EndConversationDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public EndConversationDialog(ConversationRecognizer luisRecognizer,  ILogger<EndConversationDialog> logger, MainDialog mainDialog)
            : base(nameof(EndConversationDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(mainDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                EndStepAsync,
            }));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

         private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"Are you sure you want to end our conversation?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }

        private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            string[] stringPos;
            stringPos = new string[20] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "you bet", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely" };
            string[] stringNeg;
            stringNeg = new string[8] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do" };

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

           var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            
            if(stringNeg.Any(luisResult.Text.Contains)){
               await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("It was great talking to you! Enjoy the rest of your day!", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            if(stringPos.Any(luisResult.Text.Contains)){
            var messageText = $"Great! Let's continue our conversation.";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
           return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
            var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase)";
                var elsePromptMessage2 =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  0; 
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
            

        }


       
    }
}