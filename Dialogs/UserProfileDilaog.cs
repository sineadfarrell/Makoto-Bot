using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.Dialogs

{
     public class UserProfileDialog : ComponentDialog
    {


         public UserProfileDialog()
            : base(nameof(UserProfileDialog))
    {
       

        // This array defines how the Waterfall will execute.
        var waterfallSteps = new WaterfallStep[]
        {

        NameStepAsync,
        // NameConfirmStepAsync,
        // AgeStepAsync,
        // PictureStepAsync,
        // ConfirmStepAsync,
        // SummaryStepAsync,
        };

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        // AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
        // AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        // AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
        // AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));

        // The initial child Dialog to run.
        InitialDialogId = nameof(WaterfallDialog);
        }
    private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
     {

    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
    }

    }
}