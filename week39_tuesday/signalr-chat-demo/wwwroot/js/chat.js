// elems
const message = document.getElementById('message');
const chat = document.getElementById('chat');
const sendBtn = document.getElementById('sendBtn');
const user = document.getElementById('user');
const wrapper = document.querySelector(".wrapper");

// token & conn setup
const token = sessionStorage.getItem('jwtToken');
let connection = null;

// check if token is present, decode and grab name otherwise dont allow chat
if (token) {
  const decodedJwt = JSON.parse(atob(token.split('.')[1]));
  user.innerText += decodedJwt.unique_name;
  connection = new signalR.HubConnectionBuilder()
    .withUrl('/chathub', { accessTokenFactory: () => token })
    .build();
} else {
  chat.innerText = "You are not authorized! ⛔";
  wrapper.style.display = "none";
}

// toggle send btn state
const toggleButtonState = () => {
  sendBtn.disabled = !message.value;
}

// on page load
toggleButtonState();
message.addEventListener('input', toggleButtonState);

// conn to wsocket
connection.start().then(() => {
  console.log('Connected to the hub.');
}).catch(function (err) {
  return console.error(err.toString());
});

// send msg on enter
function checkAndSendMessage(event) {
  if (event.key === "Enter" && !sendBtn.disabled) {
    sendMessage();
  }
}

// send msg binded to btn
function sendMessage() {
  const username = user.innerText.split(': ')[1];
  try {
    connection.send('SendMessage', username, message.value);
    message.value = '';
  } catch (err) {
    console.error(err.toString());
  }
}

// clear token, stop conn, go to start
function logOut() {
  if (connection) {
    connection.stop().then(() => {
      sessionStorage.removeItem('jwtToken');
      window.location.href = '/';
    }).catch(err => console.error('Error while stopping connection:', err));
  } else {
    sessionStorage.removeItem('jwtToken');
    window.location.href = '/';
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