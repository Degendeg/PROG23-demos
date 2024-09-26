const username = document.getElementById('username');
const password = document.getElementById('password');
const confirmPassword = document.getElementById('confirmPassword');

document.getElementById('signupForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  if (username.value && (password.value === confirmPassword.value)) {
    signup();
  }
});

async function signup() {
  try {
    const response = await fetch('/api/auth/signup', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        username: username.value,
        password: password.value,
        confirmPassword: confirmPassword.value
      })
    });

    if (response.ok) {
      window.location.href = '/login.html';
    } else {
      const data = await response.json();
      Toastify({
        text: data.message,
        duration: 3000
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