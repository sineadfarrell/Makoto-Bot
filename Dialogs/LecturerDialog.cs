
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
    public class LecturerDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public LecturerDialog(ConversationRecognizer luisRecognizer,  ILogger<LecturerDialog> logger, MainDialog mainDialog, EndConversationDialog endConversationDialog, ExtracurricularDialog extracurricularDialog )
            : base(nameof(LecturerDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(endConversationDialog);
            AddDialog(extracurricularDialog);
            AddDialog(mainDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                GetAnswerAsync,  
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
            var messageText = $"What's your opinion on the lecturers?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
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
           
            var moduleDetails = new ModuleDetails(){
                Lecturer = luisResult.Entities.Lecturer,
                Opinion = luisResult.Entities.Opinion,
            };
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.None)){
                var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase)";
                 var elsePromptMessage2 =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  0; 
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    
           }
            
            var messageText = $"That's interesting to know!";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
             await stepContext.Context.SendActivityAsync(elsePromptMessage, cancellationToken);
            var message = $"Would you like to talk about another aspect of university?.";
            var messageFac = new PromptOptions { Prompt = MessageFactory.Text(message, message, InputHints.ExpectingInput)};
            
            return await stepContext.PromptAsync(nameof(TextPrompt), messageFac, cancellationToken);
        }

         private async Task<DialogTurnResult> GetAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
           
            var moduleDetails = new ModuleDetails(){
                Lecturer = luisResult.Entities.Lecturer,
                Opinion = luisResult.Entities.Opinion,
            };
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.None)){
                   var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase)";
                 var elsePromptMessage2 =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  1; 
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }

            if(stringPos.Any(luisResult.Text.Contains)){
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
             if (stringNeg.Any(luisResult.Text.Contains)){
            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    
             }

               var didntUnderstandMessageText3 = $"Sorry, I didn't understand that. Could you please rephrase)";
                var elsePromptMessage3 =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  2; 
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);


        }

    }
}