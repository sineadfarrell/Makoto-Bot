// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationRecognizer luisRecognizer, UserProfileDialog userProfileDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(userProfileDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run
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
            var messageText = stepContext.Options?.ToString() ?? "Hi! I'm Makoto. I would like to talk to you about your university experience. Let's firts start with your name";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }


            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.greeting:
                   
                    // Initialize UsesrEntities with any entities we may have found in the response.
                    var userInfo = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,
                        Stage = luisResult.Entities.Stage,
                        NumberOfModules = luisResult.Entities.NumberOfModules,
                        Module = luisResult.Entities.Module,

                    };
                    return await stepContext.BeginDialogAsync(nameof(UserProfileDialog),userInfo, cancellationToken);

                case Luis.Conversation.Intent.discussModule:

                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getModuleMessageText = "TODO: get Module flow here";
                    var getModuleMessage = MessageFactory.Text(getModuleMessageText, getModuleMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getModuleMessage, cancellationToken);
                    break;
                
                case Luis.Conversation.Intent.discussLecturer:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getLecturerMessageText = "TODO: get Lecturer flow here";
                    var getLecturerMessage = MessageFactory.Text(getLecturerMessageText, getLecturerMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getLecturerMessage, cancellationToken);
                    break;

                case Luis.Conversation.Intent.discussFeeling:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getFeelingMessageText = "TODO: get Feeling flow here";
                    var getFeelingMessage = MessageFactory.Text(getFeelingMessageText, getFeelingMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getFeelingMessage, cancellationToken);
                    break;
                case Luis.Conversation.Intent.discussExtracurricular:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getCWMessageText = "TODO: get Extracurricular flow here";
                    var getCWMessage = MessageFactory.Text(getCWMessageText, getCWMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getCWMessage, cancellationToken);
                    break;
                
                case Luis.Conversation.Intent.discussCampus:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getCampusMessageText = "TODO: get Campus flow here";
                    var getCampusMessage = MessageFactory.Text(getCampusMessageText, getCampusMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getCampusMessage, cancellationToken);
                    break;
                
                case Luis.Conversation.Intent.endConversation:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getEndMessageText = "TODO: get End flow here";
                    var getEndMessage = MessageFactory.Text(getEndMessageText, getEndMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getEndMessage, cancellationToken);
                    break;
                
                case Luis.Conversation.Intent.None:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getNoneMessageText = "TODO: get None flow here";
                    var getNoneMessage = MessageFactory.Text(getNoneMessageText, getNoneMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getNoneMessage, cancellationToken);
                    break;
                
                
                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;

            }
            return await stepContext.NextAsync(null, cancellationToken);

        }
        // Shows a warning if the Modules are recognized as entities but they are not in the entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        // private static async Task ShowWarningForUnsupportedModule(ITurnContext context, Luis.Conversation luisResult, CancellationToken cancellationToken)
        // {
        //     var unsupportedModules = new List<string>();

        //     var fromEntities = luisResult.Entities;
        //     if (!string.IsNullOrEmpty(fromEntities.Module) && string.IsNullOrEmpty(fromEntities.Opinion))
        //     {
        //         unsupportedModules.Add(fromEntities.Module);
        //     }

        //     var toEntities = luisResult.Entities;
        //     if (!string.IsNullOrEmpty(toEntities.Opinion) && string.IsNullOrEmpty(toEntities.Module))
        //     {
        //         unsupportedModules.Add(toEntities.Module);
        //     }

        //     if (unsupportedModules.Any())
        //     {
        //         var messageText = $"I have never heard of {string.Join(" ", unsupportedModules)}";
        //         var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
        //         await context.SendActivityAsync(message, cancellationToken);
        //     }

        // }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInfo = (UserProfile)stepContext.Result;

            string status = "Thank you so much for talking to me today!";

            await stepContext.Context.SendActivityAsync(status);

            // var accessor = .CreateProperty<UserProfile>(nameof(UserProfile));
            // await accessor.SetAsync(stepContext.Context, userInfo, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        // private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        // {
        //     // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
        //     // the Result here will be null.
        //     if (stepContext.Result is ModuleDetails result)
        //     {

        //         var messageText = $"Thank you for telling me about {result.ModuleName} with  {result.Lecturer}";
        //         var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
        //         await stepContext.Context.SendActivityAsync(message, cancellationToken);
        //     }

        //     // Restart the main dialog with a different message the second time around
        //     var promptMessage = "What else can I do for you?";
        //     return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        // }

    }
}
