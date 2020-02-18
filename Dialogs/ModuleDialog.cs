// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ModuleDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        private const string NumberModulesMsgText = "How many modules are you doing?";
        
        public ModuleDialog(ConversationRecognizer luisRecognizer,  ILogger<ModuleDialog> logger)
            : base(nameof(ModuleDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NumberModulesStepAsync,
                NameOfModules, 
                LecturerStepAsync,
                ExamStepAsync,
                CAStepAsync,
                OpinionStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NumberModulesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            
            var moduleDetails = new ModuleDetails(){
                NumberOfModules = luisResult.Entities.NumberOfModules,
            };

            if (moduleDetails.NumberOfModules == null)
            {
                var promptMessage = MessageFactory.Text(NumberModulesMsgText, NumberModulesMsgText, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            var messageText = $"Wow {luisResult.Entities.NumberOfModules}, what is your favourite module?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> NameOfModules(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            var moduleDetails = new ModuleDetails(){
                ModuleName = luisResult.Entities.Module,
            };

            if (moduleDetails.ModuleName == null)
            {
                var ModuleMsgText = "What modules are you taking?";
                var promptMessage = MessageFactory.Text(ModuleMsgText, ModuleMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            var messageText = $"Wow {moduleDetails.NumberOfModules.GetValue(0)}, what is your favourite module?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);

        
        }
        
        private async Task<DialogTurnResult> LecturerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var moduleDetails = (ModuleDetails)stepContext.Options;

            // moduleDetails.Lecturer = (string)stepContext.Result;

            var messageText = $"Who is the lecturer for the {moduleDetails.ModuleName} module?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            // return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> ExamStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var moduleDetails = (ModuleDetails)stepContext.Options;

            // moduleDetails.Exam = (string)stepContext.Result;

            var messageText = $"Do you have a final exam in {moduleDetails.ModuleName}?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }


    private async Task<DialogTurnResult> CAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var moduleDetails = (ModuleDetails)stepContext.Options;

            // moduleDetails.ContinousAssesment = (string)stepContext.Result;

            var messageText = $"Is there a continous assesment component for the {moduleDetails.ModuleName} module?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

         private async Task<DialogTurnResult> OpinionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var moduleDetails = (ModuleDetails)stepContext.Options;

            // moduleDetails.Opinion = (string)stepContext.Result;

            var messageText = $"Do you like {moduleDetails.ModuleName}?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var moduleDetails = (ModuleDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(moduleDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
