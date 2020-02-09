using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
         public TopLevelDialog()
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
               NameStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private const string UserInfo = "value-userInfo";
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken){
        // Create an object in which to collect the user's information within the dialog.
        stepContext.Values[UserInfo] = new UserProfile(); 

    var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What's your name?") };

    // Ask the user to enter their name.
    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
}

    }
}