document.addEventListener('DOMContentLoaded', function() {

    const token = localStorage.getItem('levy_token')
    const Id = new URLSearchParams(window.location.search).get('id');
    const errorAlert = document.getElementById('error-alert');
    const errorMessage = document.getElementById('error-message');
    const successAlert = document.getElementById('success-alert');
    const successMessage = document.getElementById('success-message');

    // Reset previous messages
    errorAlert.classList.add('d-none');
    errorMessage.innerHTML = '';
    successAlert.classList.add('d-none');
    successMessage.innerHTML = '';

    fetch('http://localhost:5087/admin/api/list/students', {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
        if (response.status===200) {
            return response.json().then(data => {
                const tableBody = document.querySelector('.table-container'); // Target table body
                tableBody.innerHTML = ''; // Clear any existing rows

                if (data.$values.length === 0) {
                tableBody.innerHTML = `
                    <tr>
                    <td colspan="7" class="text-center">No data found</td>
                    </tr>
                `;
                } else {
                data.$values.forEach((student, index) => {
                    const rowHtml = `
                    <tr>
                        <th scope="row">${index + 1}</th>
                        <td>${student.firstName}</td>
                        <td>${student.lastName}</td>
                        <td>${student.matricNo}</td>
                        <td>${student.departmentName}</td>
                        <td>${new Date(student.createdAt).toLocaleDateString()}</td>
                        <td>
                            <a href="studentDetails.html?id=${student.id}" class="btn btn-info btn-sm">View Details</a>
                        </td>
                    </tr>
                    `;
                    tableBody.innerHTML += rowHtml;
                })
            }
            })
        }else{
            return response.json().then(data => {
            errorMessage.innerText = data.message || 'Unexpected error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
            })
        }
    }).catch(error => {
        errorMessage.innerText = 'Server is not responding. Please try again later.';
        errorAlert.classList.remove('d-none');
    })


    fetch('http://localhost:5087/admin/api/get/department/', {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
        if (response.status===200) {
            return response.json().then(data => {
            const selectElement = document.getElementById('departmentSelect');
            data.$values.forEach(item => {
            const option = document.createElement('option');
            option.value = item.id;  // set the value as id
            option.textContent = item.name;  // set the display text as name
            selectElement.appendChild(option);
        });
            })
        }else{
            const selectElement = document.getElementById('departmentSelect');
            selectElement.innerHTML = "unexpected error occurred"
        }
    })



    document.querySelector('.addStudent-form').addEventListener('submit', function(event) {
        event.preventDefault();
        

        const form = this;
        const formData = new FormData(form);

        // Check form validity using browser's built-in validation
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


        
        

        fetch('http://localhost:5087/admin/api/create/student/', {
            method: 'POST',
            body: JSON.stringify(Object.fromEntries(formData.entries())), // Convert form data to JSON
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        }).then(response => {
            spinner.classList.add('d-none');
            submitText.classList.remove('d-none');
            
            if (response.status===200) {
                return response.json().then(data => {
                    document.querySelector('.addStudent-form').reset();
                    successMessage.innerText = data.message;
                    successAlert.classList.remove('d-none');
                });
            } else{
                    return response.json().then(data => {
                    console.log(data.message.$values)
                    if (data.message && data.message.$values && data.message.$values.length > 0) {
                        // Extract the description from the first item in the $values array
                        const errorDescription = data.message.$values[0].description;
            
                        // Display the error message
                        errorMessage.innerText = errorDescription;
                    } else {
                        // Handle case where no error description is available
                        errorMessage.innerText = data.message || 'An unknown error occurred.';
                    }
            
                    // Show the error alert
                    errorAlert.classList.remove('d-none');
                });
            
            
            }
        })
        
        
    })


})