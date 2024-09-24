// connect to SignalR Hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/chathub')
  .build();

// elems
const user = document.getElementById('user');
const message = document.getElementById('message');
const chat = document.getElementById('chat');
const sendBtn = document.getElementById('sendBtn');

// toggle send btn state
const toggleButtonState = () => {
  sendBtn.disabled = !user.value || !message.value;
}

// on page load
toggleButtonState();
user.addEventListener('input', toggleButtonState);
message.addEventListener('input', toggleButtonState);

// conn to wsocket
connection.start().then(() => {
  console.log('Connected to the hub.');
}).catch(function (err) {
  return console.error(err.toString());
});

// send msg binded to btn
function sendMessage() {
  try {
    connection.send('SendMessage', user.value, message.value);
    message.value = '';
  } catch (err) {
    console.error(err.toString());
  }
}

// receive msgs from server, sanitize, DOM manipulate
connection.on('ReceiveMessage', (user, message) => {
  const newMessage = document.createElement('div');
  const domPurifyConf = { ALLOWED_TAGS: ['b'] };
  const sanitizedUser = DOMPurify.sanitize(user, domPurifyConf);
  const sanitizedMessage = DOMPurify.sanitize(message, domPurifyConf);

  newMessage.classList.add('message');
  newMessage.innerHTML = `<span class='username'>${sanitizedUser}:</span> ${sanitizedMessage}`;
  chat.appendChild(newMessage);
  chat.scrollTop = chat.scrollHeight;
});