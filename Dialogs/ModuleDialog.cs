// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class ModuleDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;


        public ModuleDialog(ConversationRecognizer luisRecognizer, ILogger<ModuleDialog> logger, LecturerDialog lecturerDialog, ExtracurricularDialog extracurricularDialog, EndConversationDialog endConversationDialog, FeelingDialog feelingDialog, CampusDialog campusDialog)
            : base(nameof(ModuleDialog))

        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(lecturerDialog);
            AddDialog(extracurricularDialog);
            AddDialog(endConversationDialog);
            AddDialog(feelingDialog);
            AddDialog(campusDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                NumberModulesStepAsync,
                NameOfModules,
                FinalStepAsync,
                // NextDialogAsync,
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

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken); ;
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"Excellent lets's talk about your modules. \n How many modules are you taking this trimester?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
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
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken); ;
            }
            var moduleDetails = new ModuleDetails()
            {
                NumberOfModules = luisResult.Entities.NumberOfModules,
            };
    
            //TODO : add exception if they say zero, 0, none etc
            switch (luisResult.TopIntent().intent)
            {
            case Luis.Conversation.Intent.discussModule:
           
            var messageText = $"Wow {moduleDetails.NumberOfModules.FirstOrDefault()} modules! Which one would you say is your favourite?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

            case Luis.Conversation.Intent.None:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText2 = $"Sorry, I didn't get that. Please try rephrasing your message!";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
            }
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

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken); ;
            }

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
                var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            }
            
            if(string.IsNullOrEmpty(moduleDetails.ModuleName.FirstOrDefault())){
                var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);

            }
           
            var messageText = $"Oh I've heard it's a very interesting module, what do you like about {moduleDetails.ModuleName.FirstOrDefault()}?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

            
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                ModuleName = luisResult.Entities.Module,
            };
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussLecturer))
            {
                var didntUnderstandMessageText = $"That's great! Why don't we talk about the rest of your lecturers.";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular))
            {   var didntUnderstandMessageText = $"That's great! Do you do anything in your spare time?";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussCampus))
            {   var didntUnderstandMessageText = $"That's great! Do you do like UCD's campus?";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
                var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            }


            var messageText = $"That's great! Is there a final exam for {moduleDetails.ModuleName}?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> NextDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                ModuleName = luisResult.Entities.Module,
            };
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussFeeling))
            {
                return await stepContext.BeginDialogAsync(nameof(FeelingDialog), moduleDetails, cancellationToken);
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussLecturer))
            {
                return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular))
            {
                return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussCampus))
            {
                return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
                var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            }

            var messageText = $"Would you spend much time on campus after your lectures and tutorials?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;
        }


        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
