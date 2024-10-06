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
    document.getElementById('searchInput').addEventListener('input', function() {
        fetchStudentDetails();
      });
      document.getElementById('searchInput1').addEventListener('input', function() {
        fetchStudentDetails();
      });
      document.getElementById('orderingSelect').addEventListener('change', function() {
        fetchStudentDetails();
      });
      function fetchStudentDetails() {
  
        const searchInput = document.getElementById('searchInput').value;
        const searchInput1 = document.getElementById('searchInput1').value;
        const orderSelect = document.getElementById('orderingSelect').value;

        const url = `http://localhost:5087/admin/api/get/students/details/${Id}?FilterOptions=${searchInput}&TransactionFilterOptions=${searchInput1}&OrderOptions=${orderSelect}`;
        fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token
            }
        })
        .then(response => {
            if (response.status===200) {
                return response.json();
            } else {
              return response.json().then(data => {
                  errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                  errorAlert.classList.remove('d-none');
                })
            }
          }).then(data => {
                document.getElementById("stuName").innerHTML = data.firstName + " " +  data.lastName;
                document.getElementById("stuFullName").innerHTML = data.firstName + " " +  data.lastName;
                document.getElementById("stuBalance").innerHTML = "₦"+data.balance.toFixed(2);
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
                document.getElementById("studentID4").value = data.id;
                
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
                            console.log(data)
                        const selectElement = document.getElementById('semesterID');
                        data.$values.forEach(item => {
                        const option = document.createElement('option');
                        option.value = item.id;  // set the value as id
                        option.textContent = item.name;  // set the display text as name
                        selectElement.appendChild(option);
                    });
                    const selectElement1 = document.getElementById('semesterID1');
                        data.$values.forEach(item => {
                        const option = document.createElement('option');
                        option.value = item.id;  // set the value as id
                        option.textContent = item.name;  // set the display text as name
                        selectElement1.appendChild(option);
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
                                <a href="#!" class="btn btn-secondary btn-sm" style="font-size:o.78em">Amt: ₦${item.amount.toFixed(2)}</a>
                                <a href="#!" class="btn btn-success btn-sm" >TB: ₦${item.toBalance.toFixed(2)}</a>
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
                        <th scope="row">${transaction.id}</th>
                        <td>${transaction.levyName}</td>
                        <td>₦${transaction.amount.toFixed(2)}</td>
                        <td>${transaction.description}</td>
                        <td>${transaction.payer}</td>
                        <td>${transaction.method}</td>
                        <td>${new Date(transaction.createdAt).toLocaleString()}</td>
                        <td>${transaction.transID}</td>
                    </tr>
                    `;
                    tableBody.innerHTML += rowHtml;
                })
            }
       
    }).catch(error => {
        errorMessage.innerText = 'Server is not responding. Please try again later.';
        errorAlert.classList.remove('d-none');
    })
}
fetchStudentDetails();



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
        const spinner = document.getElementById('spinner1');
        const submitText = document.getElementById('submit-text1');
        
        
        
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
        const spinner = document.getElementById('spinner2');
        const submitText = document.getElementById('submit-text2');
        
        
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
            const spinner = document.getElementById('spinner3');
            const submitText = document.getElementById('submit-text3');
            
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

    document.getElementById('downloadstudentTransaction').addEventListener('click', function() {
        const id = document.getElementById('stuID').value;
        fetch(`http://localhost:5087/admin/api/download/student/transactions/${id}`, {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + token
            }
          })
          .then(response => {
            if (response.status===200) {
                return response.blob(); // Convert the response to a Blob
            } else {
              return response.json().then(data => {
                  // Handle other error statuses
                  errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                  errorAlert.classList.remove('d-none');
                })
            }
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob); // Create a URL for the Blob
            const a = document.createElement('a'); // Create an anchor element
            a.href = url; // Set the href to the Blob URL
            a.download = `StudentTransactions.csv`; // Set the desired file name for download
            document.body.appendChild(a); // Append the anchor to the body
            a.click(); // Programmatically click the anchor to trigger the download
            a.remove(); // Remove the anchor from the DOM
            window.URL.revokeObjectURL(url); // Release the Blob URL
        })
        .catch(error => {
          errorMessage.innerText = 'An error occurred. Please try again later.';
          errorAlert.classList.remove('d-none');
        });
        
    });



    document.getElementById('uploadStudentLevies-form').addEventListener('submit', function(event) {
        event.preventDefault();
        const form = this;
        const formData = new FormData();
        // Check form validity using browser's built-in validation
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated'); // This will show the validation messages
            return;
        }

        const spinner = document.getElementById('spinner4');
        const submitText = document.getElementById('submit-text4');
        
        // Show spinner, hide login text
        spinner.classList.remove('d-none');
        submitText.classList.add('d-none');

        
        formData.append('file', document.getElementById('leviesFile').files[0]); // Append file
        formData.append('semesterId', document.getElementById('semesterID1').value); // Append department ID
        var id = document.getElementById('studentID4').value;
        fetch(`http://localhost:5087/admin/api/upload/student/bulk/levies/${id}`, {
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