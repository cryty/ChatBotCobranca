﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;
using System.Threading;
using ChatBotCobranca.Model;
using ChatBotCobranca.Utils;

namespace ChatBotCobranca.Dialogs
{
    //using Microsoft.Bot.Builder.Luis;
    //AppId,SubtringKey
    [LuisModel("57447048-5ac1-41bd-be43-159e6b4d64ca", "91d3b7c64d9e41c18eab664da8d28b67")]
    [Serializable]
    //LuisDialog<object> = using Microsoft.Bot.Builder.Dialogs
    public class DialogHub : LuisDialog<object>
    {
        
        private static QueryclientInfo _client;
        public DialogHub(QueryclientInfo client)
        {
            //Simulate some query
            _client = client;
        }

        //Luis intent
        [LuisIntent("VerFatura")]
        //"IDialogContext context" is like some data's activity
        //"LuisResult result" is your Luis.ai JSON
        public async Task VerFatura(IDialogContext context, LuisResult result)
        {
            //sending a message
            await context.PostAsync("Só um minuto por favor, estou consultando.");
            //seting a time only for the bot looks like it is typing
            Thread.Sleep(3000);
            await context.PostAsync($"Sua fatura está em R$ {_client.Fatura} senhor.");
            Thread.Sleep(3000);
            await context.PostAsync("Posso ajudar em mais alguma coisa senhor?");

            //context.PrivateConversationData.SetValue("TraitAnexo", "Fatura");

            //waitting a another message
            context.Wait(MessageReceived);

        }

        //another intent
        [LuisIntent("EnviaEmail")]
        public async Task EnviaEmail(IDialogContext context, LuisResult result)
        {
            //getting a EntityType
            EntityRecommendation entidade;
            //
            if (result.TryFindEntity("Email", out entidade))
            {
                if(result.TryFindEntity("TraitAnexo::Protocolo", out entidade)){
                    await context.PostAsync($"ok, seu protocolo será enviada no email {entidade.Entity.Replace(" ", string.Empty)}");
                }
                else if(result.TryFindEntity("TraitAnexo::Fatura", out entidade))
                {
                    await context.PostAsync($"ok, sua fatura será enviada no email {entidade.Entity.Replace(" ", string.Empty)}");
                }
                else
                {
                    await context.PostAsync($"Desculpe, pode repetir com oque voce deseja envia no seu email?");
                }
                context.Wait(MessageReceived);

            }
            else
            {
                if (result.TryFindEntity("TraitAnexo::Protocolo", out entidade)){
                    context.PrivateConversationData.SetValue("TraitAnexo", "Protocolo");
                    
                }
                else if (result.TryFindEntity("TraitAnexo::Fatura", out entidade))
                {
                    context.PrivateConversationData.SetValue("TraitAnexo", "Protocolo");
                }
                
                //promptDialog reply something to user(probabily a question) and when go to another method when the user response
                //the .confirm is when the bot expect a boolean answer(e.g.: yes or no)
                //PromptDialog.Confirm(YouContext, TheNextMethodWithoutParamers,"Your message before go to NextMethod);
                PromptDialog.Confirm(context, TrocarEmail, $"Seu email atual é:  {_client.Email} , você gostaria de trocá-lo?");
                
            }
            
           
        }

       
        private async Task TrocarEmail(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                PromptDialog.Text(context, AtualizandoEmail, "Qual é o seu e-mail?");
            }
            else
            {
                string TraitAnexo = null;
                context.PrivateConversationData.TryGetValue("TraitAnexo", out TraitAnexo);

                if (TraitAnexo == "Protocolo")
                {
                    await context.PostAsync($"Ok, enviaremos seu protcolo para o e-mail: {_client.Email}.");
                }
                else
                {
                    await context.PostAsync($"Ok, enviaremos sua fatura para o e-mail: {_client.Email}.");
                }
                context.Wait(MessageReceived);
            }
            
        }

        private async Task AtualizandoEmail(IDialogContext context, IAwaitable<string> result)
        {
            IMessageActivity Activity = context. Activity.AsMessageActivity();
            _client.updatetEmail(Activity.Text);


            if (Ultils.IsValidEmail(_client.Email))
            {
                PromptDialog.Confirm(context, ConfirmarEmail, $"Ok, você confirma que esse e-mail {_client.Email} está correto?");
            }
            else
            {
                PromptDialog.Text(context, AtualizandoEmail, $"Esse é um e-mail inválido, por favor entre com novo email valido.");
            }

        }

        private async Task ConfirmarEmail(IDialogContext context, IAwaitable<bool> result)
        {
            string TraitAnexo = null;
            if (await result)
            {
                context.PrivateConversationData.TryGetValue("TraitAnexo", out TraitAnexo);
                if(TraitAnexo == "Protocolo")
                {
                    await context.PostAsync($"Ok, enviaremos seu protcolo para o e-mail: {_client.Email}.");
                }
                else
                {
                    await context.PostAsync($"Ok, enviaremos sua fatura para o e-mail: {_client.Email}.");
                }
                context.Wait(MessageReceived);
                
            }
            else
            {
                PromptDialog.Text(context, AtualizandoEmail, "Qual é o seu e-mail?");
            }

        }

    }

        
    
}