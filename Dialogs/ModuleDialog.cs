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


        protected int turnValue = 0;

        public ModuleDialog(ConversationRecognizer luisRecognizer, ILogger<ModuleDialog> logger, LecturerDialog lecturerDialog, ExtracurricularDialog extracurricularDialog, EndConversationDialog endConversationDialog, CampusDialog campusDialog)
            : base(nameof(ModuleDialog))

        {


            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(lecturerDialog);
            AddDialog(extracurricularDialog);
            AddDialog(endConversationDialog);
            AddDialog(campusDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                NumberModulesStepAsync,
                FavModuleAsync,
                ExamorCaFavAsync,
                OpinionFavAsync,
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
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)) ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
                var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"Excellent let's talk about your modules. \n How many modules are you taking this trimester?";
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
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));
            }
            var moduleDetails = new ModuleDetails()
            {
                NumberOfModules = luisResult.Entities.NumberOfModules,
            };

            int i = 0;
            string[] numbers = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.discussModule:

                    if(moduleDetails.NumberOfModules != null) { 

                    if (int.TryParse(moduleDetails.NumberOfModules.FirstOrDefault(), out i))
                    {
                    var messageText = $"Wow {moduleDetails.NumberOfModules.FirstOrDefault()} modules! Which one would you say is your favourite?";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);
                    }

                    foreach (string x in numbers)
                    {
                        if ((moduleDetails.NumberOfModules.FirstOrDefault().ToLower()).Contains(x))
                        {
                    var messageText = $"Wow {moduleDetails.NumberOfModules.FirstOrDefault()} modules! Which one would you say is your favourite?";
                    var elsePromptMessageQ= new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageQ, cancellationToken);
                        }
                    }

                }
                var didntUnderstandMessageTextK = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessageK = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageTextK, didntUnderstandMessageTextK, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageK, cancellationToken);

                case Luis.Conversation.Intent.None:
                 var didntUnderstandMessageText = $"Sorry, I didn't understand that. Could you please rephrase";
                var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText3 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }

        }

        private async Task<DialogTurnResult> FavModuleAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog) );


                case Luis.Conversation.Intent.discussModule:
                    var messageText = " ";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };

                if(moduleDetails.ModuleName != null){
                    messageText = $"Ah very good! I've heard it's a very interesting module, is there a final exam for {moduleDetails.ModuleName.FirstOrDefault()}?";
                    elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };


                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
                }
                 var didntUnderstandMessageText3 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    //turnValue += 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);

                // Catch all for unhandled intents
                // Catch all for unhandled intents
                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    //turnValue += 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                default:
                 messageText = " ";
                 elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };


                    messageText = $"Ah very good! I've heard it's a very interesting module, is there a final exam for this module?";
                    elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };


                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);


            }

        }

        private async Task<DialogTurnResult> ExamorCaFavAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); 


                case Luis.Conversation.Intent.discussModule:
                    var messageText = $"Ok! Why do you like the module?";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                case Luis.Conversation.Intent.None:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;

                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                default:
                    var messageText2 = $"Ok! Why do you like the module?";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);

            }
        }
        
        private async Task<DialogTurnResult> OpinionFavAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken); ;

               

                    case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase)";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                     default:
                    var messageText2 = $"That's great! Why don't we talk about your lecturers for a bit.";
                    var elsePromptMessage =  MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) ;

                    await stepContext.Context.SendActivityAsync(elsePromptMessage, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(LecturerDialog)); 
                // Catch all for unhandled intents

            }
        

       
    }
}
}
