const conversationsWrapper = document.getElementById('conversations');
const chatWindow = document.getElementById('chat-window');
const privateMessageInput = document.getElementById('privateMessage');
let currentConversationId = null;

function openPrivateChat(conversationId, username, messages) {
  currentConversationId = conversationId;
  chatWindow.innerHTML = ''; // Clear previous content
  privateMessageInput.placeholder = `Chat with ${username}...`;

  // Show prev messages
  if (messages) {
    messages.forEach(message => {
      const newMessage = document.createElement('div');
      newMessage.innerHTML = `<strong>${message.username}:</strong> ${message.message}`;
      chatWindow.appendChild(newMessage);
    });
  }

  conversationsWrapper.style.display = 'block';
  chatWindow.scrollTop = chatWindow.scrollHeight; // Scroll to bottom
}

function closePrivateChat() {
  // Reset the current convo ID
  currentConversationId = null;
  chatWindow.innerHTML = '';
  conversationsWrapper.style.display = 'none';
}

// Listen for a username click to start a private chat
document.addEventListener('click', function (event) {
  if (event.target.classList.contains('username')) {
    const targetUser = event.target.innerText;
    if (targetUser !== sessionStorage.getItem('username')) {
      connection.send('StartPrivateChat', targetUser)
        .catch(err => console.error(err.toString()));
    } else {
      alert('No conversations with yourself this time, try again!');
    }
  }
});

// Listen for the private chat to open and load previous messages
connection.on('OpenPrivateChat', (conversationId, username, messages) => {
  openPrivateChat(conversationId, username, messages);
});

// Send a private message
function sendPrivateMessage() {
  if (currentConversationId) {
    const message = privateMessageInput.value;

    if (message.trim() !== '') {
      const newMessage = {
        Username: sessionStorage.getItem('username'),
        Message: message
      };

      // displayMessage(newMessage);
      privateMessageInput.value = '';

      connection.send('SendPrivateMessage', currentConversationId, message)
        .catch(err => console.error(err.toString()));
    }
  }
}

// Function to display a message in the chat window
function displayMessage(message) {
  const newMessageElement = document.createElement('div');
  newMessageElement.innerHTML = `<strong>${message.Username}:</strong> ${message.Message}`;
  chatWindow.appendChild(newMessageElement);
  chatWindow.scrollTop = chatWindow.scrollHeight; // Scroll to bottom
}

// Receive and display a new private message
connection.on('ReceivePrivateMessage', (conversationId, sender, message) => {
  if (conversationId === currentConversationId) {
    const newMessage = {
      Username: sender,
      Message: message
    };

    displayMessage(newMessage);
  }
});