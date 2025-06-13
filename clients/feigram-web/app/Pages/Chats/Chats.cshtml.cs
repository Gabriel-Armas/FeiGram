using app.ViewModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace app.Pages.Chats
{
    public class ChatModel : PageModel
    {
        public List<FriendViewModel> Friends { get; set; } = new List<FriendViewModel>();
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public FriendViewModel SelectedFriend { get; set; }

        public void OnGet(int? friendId)
        {
            Friends = GetFriends();

            if (friendId.HasValue)
            {
                SelectedFriend = Friends.FirstOrDefault(f => f.Id == friendId.Value);
                Messages = GetMessages(friendId.Value);
            }
        }

        private List<FriendViewModel> GetFriends()
        {
            return new List<FriendViewModel>
            {
                new FriendViewModel { Id = 1, Username = "Naruto", LastMessagePreview = "Dattebayo!", ProfilePictureUrl = "https://i.pinimg.com/originals/39/9b/24/399b244bf6f544e2c82230cfbb65be27.png" },
                new FriendViewModel { Id = 2, Username = "Sasuke", LastMessagePreview = "I'm leaving...", ProfilePictureUrl = "/images/sasuke.png" }
            };
        }

        private List<MessageViewModel> GetMessages(int friendId)
        {
            var allMessages = new Dictionary<int, List<MessageViewModel>>
            {
                {
                    1, new List<MessageViewModel>
                    {
                        new MessageViewModel { Sender = "Naruto", Text = "Hey! Ready for the next mission?" },
                        new MessageViewModel { Sender = "You", Text = "Absolutely!" }
                    }
                },
                {
                    2, new List<MessageViewModel>
                    {
                        new MessageViewModel { Sender = "Sasuke", Text = "I must go now." },
                        new MessageViewModel { Sender = "You", Text = "Be safe, bro." }
                    }
                }
            };

            return allMessages.ContainsKey(friendId) ? allMessages[friendId] : new List<MessageViewModel>();
        }
    }
}
