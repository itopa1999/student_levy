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
    fetch(`http://localhost:5087/admin/api/get/levy/details/${Id}`, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
        if (response.status===200) {
            return response.json().then(data => {
                console.log(data)
                document.getElementById('semesterName').innerHTML = data.name;
                document.getElementById('semesterDepartment').innerHTML = data.departmentName;
                document.getElementById('semesterID').value = data.id;
                document.getElementById('semesID').value = data.id;
                document.getElementById("semesterlink").addEventListener('click', function(event) {
                    event.preventDefault();
                    let id = data.departmentID;
                    this.href = `departmentDetails.html?id=${id}`;
                    window.location.href = this.href;
                });
                
                const container = document.getElementById('semester-container');
                container.innerHTML =""
                if (data.levies.$values.length === 0) {
                    const cardHtml1 = `
                      <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                      </div>
                    `;
                    container.innerHTML += cardHtml1;
                }else{
                    {data.levies.$values.forEach(item => {
                    const cardHtml = `
                    <li class="list-group-item d-flex justify-content-between align-items-start">
                      <div class="ms-2 me-auto">
                        <div class="fw-bold">${item.name}</div>
                        <span style="font-size: 0.85rem;">CreatedAt: ${item.createdAt}</span>
                      </div>
                      <a href="#!" class="btn btn-secondary btn-sm" >₦${item.amount.toFixed(2)}</a>
                    </li>
                        `;
                        container.innerHTML += cardHtml;
                    });
                }
            }

            })
        
        }else {
            return response.json().then(data => {
            // Handle other error statuses
            errorMessage.innerText = 'An error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
            })
        }
    });


    document.querySelector('.addLevy-form').addEventListener('submit', function(event) {
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
        
        // Show spinner, hide login text
        spinner.classList.remove('d-none');
        submitText.classList.add('d-none');

        fetch('http://localhost:5087/admin/api/create/levy', {
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
                    document.querySelector('.addLevy-form').reset();
                    successMessage.innerText = data.message;
                    successAlert.classList.remove('d-none');
                    
                });
            
            } else {
                return response.json().then(data => {
                // Handle other error statuses
                errorMessage.innerText = 'An error occurred. Please try again later.';
                errorAlert.classList.remove('d-none');
                })
            }
        })
        
    });


    document.getElementById('uploadLevies-form').addEventListener('submit', function(event) {
        event.preventDefault();
        const form = this;
        const formData = new FormData();
        // Check form validity using browser's built-in validation
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated'); // This will show the validation messages
            return;
        }

        const spinner = document.getElementById('spinner1');
        const submitText = document.getElementById('submit-text1');
        
        // Show spinner, hide login text
        spinner.classList.remove('d-none');
        submitText.classList.add('d-none');

        
        formData.append('file', document.getElementById('leviesFile').files[0]); // Append file
        formData.append('semesterId', document.getElementById('semesID').value); // Append department ID

        fetch('http://localhost:5087/admin/api/upload/levies', {
            method: 'POST',
            body: formData,
            headers: {
                'Authorization': 'Bearer ' + token
            }
        })
        .then(response => {
            spinner.classList.add('d-none');
            submitText.classList.remove('d-none');
            if (response.status===200) {
                return response.json().then(data => {
                successMessage.innerText = data.message;
                successAlert.classList.remove('d-none');
                })
            } else {
              return response.json().then(data => {
                  errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                  errorAlert.classList.remove('d-none');
                })
            }
        })
        .catch(error => {
            errorMessage.innerText = ('An error occurred. Please try again later.');
            errorAlert.classList.remove('d-none');
        });

        
        })


})
