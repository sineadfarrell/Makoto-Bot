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
        public MainDialog(ConversationRecognizer luisRecognizer, ExtracurricularDialog extracurricularDialog, CampusDialog campusDialog,  ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
    
            AddDialog(campusDialog);
            AddDialog(extracurricularDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run
            InitialDialogId = nameof(WaterfallDialog);
        }

    private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("So what would you like to talk about? For example we can talk about extracurricular activities or UCD campus?")}, cancellationToken);
            }
       
        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }


            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.endConversation:
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
            return await stepContext.BeginDialogAsync(nameof(CampusDialog));

                case Luis.Conversation.Intent.discussCampus:
                    var moduleInfoCampus = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion
                    };
                    return await stepContext.BeginDialogAsync(nameof(CampusDialog));
                
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
                    var didntUnderstandMessageText2 = $"Sorry, I didn't understand. Let's try again!";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);
                   
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));

                default:
                    // Catch all for unhandled intents
                var didntUnderstandMessageTextNone = $"Sorry, I didn't understand that. Could you please rephrase";
                 var elsePromptMessageNone =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageTextNone, didntUnderstandMessageTextNone, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  0; 
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNone, cancellationToken);

            }
           

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }

    }
}
