document.addEventListener('DOMContentLoaded', function() {
    document.querySelector('.addDepartment-form').addEventListener('submit', function(event) {
        event.preventDefault();

        const form = this;

        // Check form validity using browser's built-in validation
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated'); // This will show the validation messages
            return;
        }
        const formData = new FormData(form);
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

        // Send the login request
        fetch('http://localhost:5087/admin/api/create/department', {
            method: 'POST',
            body: JSON.stringify(Object.fromEntries(formData.entries())), // Convert form data to JSON
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => {
            spinner.classList.add('d-none');
            submitText.classList.remove('d-none');
            
            if (response.ok) {
                return response.json().then(data => {
                    successMessage.innerText = data.message;
                    successAlert.classList.remove('d-none');
                });
            } else if (response.status === 400) {
                return response.json().then(data => {
                    // Display error message for incorrect credentials
                    errorMessage.innerText = data.message;
                    errorAlert.classList.remove('d-none');
                });
            } else {
                // Handle other error statuses
                errorMessage.innerText = 'An error occurred. Please try again later.';
                errorAlert.classList.remove('d-none');
            }
        })
        
    });

    

});