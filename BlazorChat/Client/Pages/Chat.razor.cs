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
        private string searchValue = "";
        private string searchMessageValue = "";
        [CascadingParameter] public HubConnection hubConnection { get; set; }
        [Parameter] public string CurrentMessage { get; set; }
        [Parameter] public string CurrentUserId { get; set; }
        [Parameter] public string CurrentUserName { get; set; }
        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<ChatMessage> allMessages = new List<ChatMessage>();
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

        private string GetShortString(string text)
        {
            if (String.IsNullOrEmpty(text)) return "";
            if (text.Length < 15) return text;
            return text.Substring(0, 14) + "...";
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await _jsRuntime.InvokeAsync<string>("ScrollToBottom", "chatContainer");
        }
        protected override async Task OnInitializedAsync()
        {
            if (hubConnection == null)
            {
                hubConnection = new HubConnectionBuilder().WithUrl(_navigationManager.ToAbsoluteUri("/signalRHub")).Build();
            }
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                await hubConnection.StartAsync();
            }
            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            CurrentUserId = user.Claims.Where(a => a.Type == "sub").Select(a => a.Value).FirstOrDefault();
            CurrentUserName = user.Claims.Where(a => a.Type == "name").Select(a => a.Value).FirstOrDefault();

            hubConnection.On<ChatMessage, string>("ReceiveMessage", async (message, userName) =>
            {
                if ((ContactId == message.ToUserId && CurrentUserId == message.FromUserId) || (ContactId == message.FromUserId && CurrentUserId == message.ToUserId))
                {
                    if ((ContactId == message.ToUserId && CurrentUserId == message.FromUserId))
                    {
                        var newMessage = new ChatMessage
                        {
                            Message = message.Message,
                            CreatedDate = message.CreatedDate,
                            ToUserId = message.ToUserId,
                            FromUserId = message.FromUserId,
                            FromUser = new ApplicationUser() { UserName = CurrentUserName }
                        };
                        messages.Add(newMessage);
                        allMessages.Add(newMessage);
                        await hubConnection.SendAsync("ChatNotificationAsync", $"Новое сообщение от {userName}:\n{message.Message}...", ContactId, CurrentUserId);
                    }
                    else if ((ContactId == message.FromUserId && CurrentUserId == message.ToUserId))
                    {
                        var newMessage = new ChatMessage
                        {
                            Message = message.Message,
                            CreatedDate = message.CreatedDate,
                            ToUserId = message.ToUserId,
                            FromUserId = message.FromUserId,
                            FromUser = new ApplicationUser() { UserName = ContactName }
                        };
                        messages.Add(newMessage);
                        allMessages.Add(newMessage);
                    }
                    await _jsRuntime.InvokeAsync<string>("ScrollToBottom", "chatContainer");
                    StateHasChanged();
                }
            });

            await GetUsersAsync();
            foreach (var chatUser in ChatUsers)
            {
                var contact = await _chatManager.GetUserDetailsAsync(chatUser.Id);
                var contactId = contact.Id;
                var result = await _chatManager.GetConversationAsync(contactId);
                foreach (var message in result)
                {
                    allMessages.Add(message);
                }
            }
            if (!string.IsNullOrEmpty(ContactId))
            {
                await LoadUserChat(ContactId);
            }
            StateHasChanged();
        }

        async Task LoadUserChat(string userId)
        {
            var contact = await _chatManager.GetUserDetailsAsync(userId);
            ContactId = contact.Id;
            ContactName = contact.UserName;
            _navigationManager.NavigateTo($"chat/{ContactId}");

            messages = await _chatManager.GetConversationAsync(ContactId);
        }


        private async Task GetUsersAsync()
        {
            ChatUsers = await _chatManager.GetUsersAsync();
        }
    }
}
