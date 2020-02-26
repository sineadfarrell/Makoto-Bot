
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ExtracurricularDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public ExtracurricularDialog(ConversationRecognizer luisRecognizer,  ILogger<ExtracurricularDialog> logger, LecturerDialog lecturerDialog, CampusDialog campusDialog, ExtracurricularDialog extracurricularDialog, ModuleDialog moduleDialog, EndConversationDialog endConversationDialog)
            : base(nameof(ExtracurricularDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(endConversationDialog);
            AddDialog(moduleDialog);
            AddDialog(extracurricularDialog);
            AddDialog(lecturerDialog);
            AddDialog(campusDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                MoveConvoAsync,
                
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
            var messageText = $"What do you do in your spare time on campus";
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
            
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken);;    
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular)){
            var messageText = $"Wow that's great!";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }
           
            var messageText2 = $"Is there anything else you'd like to talk about?";
            var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
        }
    
    private async Task<DialogTurnResult> MoveConvoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
            case Luis.Conversation.Intent.discussModule:
            

                    var moduleInfo = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion,

                    };

                    return await stepContext.BeginDialogAsync(nameof(ModuleDialog), moduleInfo, cancellationToken);

                case Luis.Conversation.Intent.endConversation:
                    var moduleInfoEnd = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion,

                    };
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleInfoEnd, cancellationToken);

                case Luis.Conversation.Intent.discussLecturer:
                    var moduleInfoLec = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion,

                    };
                    return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleInfoLec, cancellationToken);

                case Luis.Conversation.Intent.discussCampus:
                    var moduleInfoCampus = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion
                    };
                    return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleInfoCampus, cancellationToken);

                case Luis.Conversation.Intent.discussExtracurricular:

                    var moduleInfoExtra = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion
                    };
                    return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleInfoExtra, cancellationToken);

                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);


             default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText2 = $"Sorry, I didn't get that. Please try rephrasing your message!";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);
                    break;
        }
          return await stepContext.EndDialogAsync();

    }
    }
}