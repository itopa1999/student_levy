document.addEventListener('DOMContentLoaded', function() {
    const token = localStorage.getItem('levy_token')
    if (token == null && token == 'undefined') {
      window.location.href = 'login.html';
    }
    const errorAlert = document.getElementById('error-alert');
    const errorMessage = document.getElementById('error-message');
    const successAlert = document.getElementById('success-alert');
    const successMessage = document.getElementById('success-message');

    // Reset previous messages
    errorAlert.classList.add('d-none');
    errorMessage.innerHTML = '';
    successAlert.classList.add('d-none');
    successMessage.innerHTML = '';


    fetch('http://localhost:5087/student/api/student/dashboard', {
      method: 'GET',
      headers: {
          'Authorization': 'Bearer ' + token
      }
    }).then(response => {
        if (response.ok) {
          return response.json().then(data => {
            document.getElementById('stuBalance').innerHTML = data.stu_balance;
          })
        }else {
            // Handle other error statuses
            errorMessage.innerText = 'An error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
          }
    })

})