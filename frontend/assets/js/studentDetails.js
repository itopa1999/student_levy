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
    fetch(`http://localhost:5087/admin/api/get/students/details/${Id}`, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    }).then(response => {
        if (response.status===200) {
            return response.json().then(data => {
                document.getElementById("stuName").innerHTML = data.firstName + " " +  data.lastName;
                document.getElementById("stuFullName").innerHTML = data.firstName + " " +  data.lastName;
                document.getElementById("stuBalance").innerHTML = data.balance;
                document.getElementById("stuFirstName").innerHTML = data.firstName;
                document.getElementById("stuLastName").innerHTML = data.lastName;
                document.getElementById("stuMatric").innerHTML = data.matricNo;
                document.getElementById("stuUsername").innerHTML = data.username;
                document.getElementById("stuDepartment").innerHTML = data.departmentName;
                document.getElementById("firstname").value = data.firstName;
                document.getElementById("lastname").value = data.lastName;
                document.getElementById("matricno").value = data.matricNo;
                document.getElementById("stuID").value = data.id;
                document.getElementById("studentId").value = data.id;
                document.getElementById("studentIDs").value = data.id;
                var id =data.id;
                
                fetch(`http://localhost:5087/admin/api/get/student/semester/${id}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': 'Bearer ' + token
                    }
                })
                .then(response => {
                    if (response.status===200) {
                        return response.json().then(data => {
                        const selectElement = document.getElementById('semesterID');
                        data.$values.forEach(item => {
                        const option = document.createElement('option');
                        option.value = item.id;  // set the value as id
                        option.textContent = item.name;  // set the display text as name
                        selectElement.appendChild(option);
                    });
                        })
                    }else{
                        const selectElement = document.getElementById('semesterID');
                        selectElement.innerHTML = "unexpected error occurred"
                    }
                })
                const container = document.getElementById('accordionFlushExample');
                container.innerHTML = ""
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
                    <div class="accordion-item">
                            <h2 class="accordion-header" id="flush-heading${item.id}">
                              <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#flush-collapse${item.id}" aria-expanded="false" aria-controls="flush-collapse${item.id}">
                              ${item.semesterName} Levy
                              </button>
                            </h2>
                            <div id="flush-collapse${item.id}"  class="accordion-collapse show active collapse" aria-labelledby="flush-headingOne" data-bs-parent="#accordionFlushExample">
                              <div class="accordion-body">
                              <li class="list-group-item d-flex justify-content-between align-items-start">
                                <div class="ms-2 me-auto">
                                    <div class="fw-bold">${item.name}</div>
                                    <span style="font-size: 0.85rem;">CreatedAt: ${item.createdAt}</span>
                                    <div class="mt-2"><a href="#" class="btn btn-warning btn-sm" data-bs-toggle="modal" data-bs-target="#PayModal" 
                                       data-id="${item.id}" data-toBalance="${item.toBalance}" data-name="${item.name}" data-semester="${item.semesterName}">
                                       pay
                                    </a></div>
                                </div>
                                <a href="#!" class="btn btn-secondary btn-sm" style="font-size:o.78em">Amt: ${item.amount}</a>
                                <a href="#!" class="btn btn-success btn-sm" >TB ${item.toBalance}</a>
                                </li>
                              </div>
                            </div>
                          </div>
                        `;
                        container.innerHTML += cardHtml;
                    });
                }
            }
                const updateModal = document.getElementById('PayModal');
                updateModal.addEventListener('show.bs.modal', event => {
                    const button = event.relatedTarget; // Button that triggered the modal
                    const id = button.getAttribute('data-id');
                    const name = button.getAttribute('data-name');
                    const toBalance = button.getAttribute('data-toBalance');
                    const semester = button.getAttribute('data-semester');

                    
                    const itemIdInput = document.getElementById('itemId');
                    const itemAmountInput = document.getElementById('itemAmount');
                    const title = document.getElementById('title');
                    const description = document.getElementById('itemDescription');
                    

                    itemIdInput.value = id;
                    itemAmountInput.value = toBalance;
                    itemAmountInput.max = toBalance;
                    description.value = name +" Payment for "+ semester
                    title.innerHTML = "Pay for "+ semester + " " + name + " (ToBalance: " + toBalance +" )";
                });

                const tableBody = document.querySelector('.table-container'); // Target table body
                tableBody.innerHTML = ''; // Clear any existing rows

                if (data.transactions.$values.length === 0) {
                tableBody.innerHTML = `
                    <tr>
                    <td colspan="7" class="text-center">No data found</td>
                    </tr>
                `;
                } else {
                data.transactions.$values.forEach((transaction, index) => {
                    const rowHtml = `
                    <tr>
                        <th scope="row">${index + 1}</th>
                        <td>${transaction.levyName}</td>
                        <td>${transaction.amount}</td>
                        <td>${transaction.description}</td>
                        <td>${transaction.method}</td>
                        <td>${new Date(transaction.createdAt).toLocaleString()}</td>
                        <td>${transaction.transID}</td>
                    </tr>
                    `;
                    tableBody.innerHTML += rowHtml;
                })
            }


            });
        }else {
            return response.json().then(data => {
            errorMessage.innerText = data.message || 'Unexpected error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
            })
        }
    }).catch(error => {
        errorMessage.innerText = 'Server is not responding. Please try again later.';
        errorAlert.classList.remove('d-none');
    })


    document.querySelector('.payStudent-form').addEventListener('submit', function(event) {
        const id = document.getElementById('itemId').value;
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
        fetch(`http://localhost:5087/admin/api/pay/student/levy`, {
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
                    successMessage.innerText = data.message || "Payment successfully";
                    successAlert.classList.remove('d-none');
                });
            
            }else {
                return response.json().then(data => {
                errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                errorAlert.classList.remove('d-none');
                })
            }
        })
        

    })


    

    document.querySelector('.updateStudent-form').addEventListener('submit', function(event) {
        const id = document.getElementById('stuID').value;
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
        fetch(`http://localhost:5087/admin/api/update/student/details/${id}`, {
            method: 'PUT',
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
                    successMessage.innerText = data.message || "updated successfully";
                    successAlert.classList.remove('d-none');
                });
            }else {
                errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                errorAlert.classList.remove('d-none');
            };
        });


    });



    document.querySelector('.changePassword-form').addEventListener('submit', function(event) {
        const id = document.getElementById('stuID').value;
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
        fetch(`http://localhost:5087/admin/api/change/student/password/${id}`, {
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
        });

    })



        document.querySelector('.addStudentLevy-form').addEventListener('submit', function(event) {
            const id = document.getElementById("studentIDs").value
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
            fetch(`http://localhost:5087/admin/api/add/levy/student/${id}`, {
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
                        successMessage.innerText = data.message || "Created successfully";
                        successAlert.classList.remove('d-none');
                    });
                }else{
                    return response.json().then(data => {
                        if (data.message && data.message.$values && data.message.$values.length > 0) {
                            // Extract the description from the first item in the $values array
                            const errorDescription = data.message.$values[0].description;
                
                            // Display the error message
                            errorMessage.innerText = errorDescription;
                        } else {
                            // Handle case where no error description is available
                            errorMessage.innerText = data.message || 'An error occurred. Please try again later.';
                        }
                        errorAlert.classList.remove('d-none');
                });
                }
            });


    });

                


})