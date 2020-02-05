// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.Bot.Builder;
// using Microsoft.Bot.Builder.Dialogs;
// using Microsoft.Bot.Schema;



// namespace Microsoft.BotBuilderSamples.Dialogs
// {
//      public static class Responses 
//     {
//         //greeting response
//         public async static Task Send_Greeting(IDialogContext context, IMessageActivity message)
//         {
//             var reply = CreateResponse(
//                             context,
//                             message,
//                             "Hi, I'm Makoto",
//                             "Hi, I'm Makoto",
//                             messageType: MessageType.Statement,
//                             inputHint: InputHints.IgnoringInput);

//             await context.PostAsync(reply);
//         }


//         //goodbye response

//         //did not undersatnd response

//         //select random reply 
//         private static string SelectRandomReply(IList<string> options)
//         {
//             var rand = new Random();
//             var index = rand.Next(0, options.Count - 1);
//             return options[index];
//         }

//     }



// }
