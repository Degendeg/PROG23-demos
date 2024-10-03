const username = document.getElementById('username');
const password = document.getElementById('password');
const loader = document.getElementById('loader');

document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  if (username.value && password.value) {
    loader.style.display = 'flex'; // show ⟳
    await login();
    loader.style.display = 'none'; // hide ⟳
  }
});

async function login() {
  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        username: username.value,
        password: password.value
      })
    });

    if (response.ok) {
      const data = await response.json();
      sessionStorage.setItem('jwtToken', data.token);
      Toastify({
        text: data.message,
        duration: 2000,
      }).showToast();
      setTimeout(() => window.location.href = '/chat.html', 666);
    } else {
      Toastify({
        text: "Username or password is incorrect, try again.",
        duration: 3000,
      }).showToast();
    }
  } catch (error) {
    console.error('Error:', error);
    Toastify({
      text: "An unexpected error occurred. Please try again.",
      duration: 3000
    }).showToast();
  }
}

async function handleGoogleSignIn(response) {
  try {
    // Extract the token from Google's response
    const googleToken = response.credential;

    // Send the Google token to your backend
    const serverResponse = await fetch('/api/auth/google-callback', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ token: googleToken })
    });

    if (serverResponse.ok) {
      const data = await serverResponse.json();
      sessionStorage.setItem('jwtToken', data.token);

      Toastify({
        text: "Google login successful",
        duration: 2000,
      }).showToast();

      // Redirect to chat page
      setTimeout(() => window.location.href = '/chat.html', 666);
    } else {
      Toastify({
        text: "Google login failed, try again.",
        duration: 3000,
      }).showToast();
    }
  } catch (error) {
    console.error('Error during Google login:', error);
    Toastify({
      text: "An error occurred. Please try again.",
      duration: 3000
    }).showToast();
  }
}