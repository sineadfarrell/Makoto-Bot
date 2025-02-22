using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using Microsoft.Extensions.Logging;


namespace Microsoft.BotBuilderSamples.Dialogs
{
    // Class added to discuss Covid-19
    public class CoronaDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public CoronaDialog(ConversationRecognizer luisRecognizer, ILogger<CoronaDialog> logger)
            : base(nameof(CoronaDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ExplanationStepAsync,
                FinalStepAsync,
                
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
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
           

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"How do you feel about the closure of UCD because of the virus?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }

        private async Task<DialogTurnResult> ExplanationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
             if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                string[] stringPos;
                stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
                string[] stringNeg;
                stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

                var messageTextEnd = $"Are you sure you want to end our conversation?";
                var elsePromptMessageEnd = new PromptOptions { Prompt = MessageFactory.Text(messageTextEnd, messageTextEnd, InputHints.ExpectingInput)};
                await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageEnd, cancellationToken);
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
            var messageText = $"What kind of activities are you doing at home now?";

            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

        }

private async Task<DialogTurnResult> ResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
              if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
             if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                string[] stringPos;
                stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
                string[] stringNeg;
                stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

                var messageTextEnd = $"Are you sure you want to end our conversation?";
                var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageTextEnd, messageTextEnd, InputHints.ExpectingInput)};
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
            var messageText = $"That's awesome! Well that's all we have time to talk about! ";
          
            await stepContext.Context.SendActivityAsync( MessageFactory.Text(messageText, inputHint: InputHints.IgnoringInput), cancellationToken);
            return await stepContext.NextAsync();

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("It was great talking to you! Enjoy the rest of your day!", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            


        }
    }
    }