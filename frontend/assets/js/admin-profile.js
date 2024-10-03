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

    fetch('http://localhost:5087/admin/api/admin/profile/details', {
      method: 'GET',
      headers: {
          'Authorization': 'Bearer ' + token
      }
    }).then(response => {
        if (response.status===200) {
            return response.json();
        } else {
          return response.json().then(data => {
              // Handle other error statuses
              errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
              errorAlert.classList.remove('d-none');
            })
        }
    }).then(data => {
        console.log(data)
        document.getElementById('adminName').innerHTML = data.firstName + " " + data.lastName;
        document.getElementById('adminFullName').innerHTML = data.firstName + " " + data.lastName;
        document.getElementById('adminFirstName').innerHTML = data.firstName;
        document.getElementById('adminLastName').innerHTML = data.lastName;
        document.getElementById('adminUsername').innerHTML = data.username;
    }).catch(error => {
        errorMessage.innerText = 'Server is not responding. Please try again later.';
        errorAlert.classList.remove('d-none');
    })



    document.querySelector('.AdminChangePassword-form').addEventListener('submit', function(event) {
        event.preventDefault();
        const form = this;
        const formData = new FormData(form);
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated'); // This will show the validation messages
            return;
        }
        const spinner = document.getElementById('spinner');
        const submitText = document.getElementById('submit-text');
        const errorAlert = document.getElementById('error-alert');
        const errorMessage = document.getElementById('error-message');
        const successAlert = document.getElementById('success-alert');
        const successMessage = document.getElementById('success-message');

        // Reset previous messages
        errorAlert.classList.add('d-none');
        errorMessage.innerHTML = '';
        successAlert.classList.add('d-none');
        successMessage.innerHTML = '';
        
        // Show spinner, hide login text
        spinner.classList.remove('d-none');
        submitText.classList.add('d-none');
    fetch('http://localhost:5087/auth/api/change/password', {
        method: 'POST',
            body: JSON.stringify(Object.fromEntries(formData.entries())), // Convert form data to JSON
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        })
        .then(response => {
            spinner.classList.add('d-none');
            submitText.classList.remove('d-none');
            
            if (response.status===200) {
                return response.json().then(data => {
                    successMessage.innerText = data.message || "changed successfully";
                    successAlert.classList.remove('d-none');
                });
            }else {
                return response.json().then(data => {
                    if (data.message && data.message.$values && data.message.$values.length > 0) {
                        // Extract the description from the first item in the $values array
                        const errorDescription = data.message.$values[0].description;
            
                        // Display the error message
                        errorMessage.innerText = errorDescription;
                    } else {
                        // Handle case where no error description is available
                        errorMessage.innerText = data.message || 'Failed to change password.';
                    }
                    errorAlert.classList.remove('d-none');
            });
        }
        })
    })
    

})
