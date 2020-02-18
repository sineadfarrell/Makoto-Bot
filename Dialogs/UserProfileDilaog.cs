using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs

{
    public class UserProfileDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        public UserProfileDialog(ConversationRecognizer luisRecognizer,  UserProfileDialog userProfileDialog,  ILogger<ModuleDialog> logger, UserState userState, ModuleDialog moduleDialog)
            : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            AddDialog(moduleDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {

            NameStepAsync,
            NumberOfModulesAsync,

            }));

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.

            


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // stepContext.Values["stage"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Hello! Could you please tell me your name.") }, cancellationToken);
        }
         private async Task<DialogTurnResult> NumberOfModulesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            var userInfo = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,

                    };
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {userInfo.Name}, it's great to meet you! Let's talk about your modules"), cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(ModuleDialog), userInfo, cancellationToken);
            }

           
        
    }

       
}