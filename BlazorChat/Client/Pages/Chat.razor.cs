using BlazorChat.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorChat.Client.Pages
{
    public partial class Chat
    {
       
        [CascadingParameter] public HubConnection hubConnection { get; set; }
        [Parameter] public string CurrentMessage { get; set; }
        [Parameter] public string CurrentUserId { get; set; }
        [Parameter] public string CurrentUserName { get; set; }

       
        public List<ApplicationUser> ChatUsers = new List<ApplicationUser>();
        [Parameter] public string ContactName { get; set; }
        [Parameter] public string ContactId { get; set; }
        private async Task SubmitAsync()
        {
            if (!string.IsNullOrEmpty(CurrentMessage) && !string.IsNullOrEmpty(ContactId))
            {
                
                var chatHistory = new ChatMessage()
                {
                    Message = CurrentMessage,
                    ToUserId = ContactId,
                    CreatedDate = DateTime.Now

                };
                await _chatManager.SaveMessageAsync(chatHistory);
                chatHistory.FromUserId = CurrentUserId;
                await hubConnection.SendAsync("SendMessageAsync", chatHistory, CurrentUserName);
                CurrentMessage = string.Empty;
            }
        }
        
    }
}
