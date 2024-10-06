document.addEventListener('DOMContentLoaded', function() {

    const token = localStorage.getItem('levy_token')
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
        fetchDepartment();
      });

    function fetchDepartment() {
    const searchInput = document.getElementById('searchInput').value;
    const url = `http://localhost:5087/admin/api/list/department?FilterOptions=${searchInput}`
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
    })
    .then(data => {
        console.log(data)
                const container = document.getElementById('card-container');
                container.innerHTML =""
                if (data.$values.length === 0) {
                    container.innerHTML = `
                        <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                        </div>
                    `;
                } else {data.$values.forEach(item => {
                    const cardHtml = `
                        <div class="col-xxl-4 col-md-4">
                        <div class="card info-card sales-card">
                            <div class="card-body">
                            <h5 class="card-title"><strong>${item.name}</strong></h5>
                            <div class="d-flex align-items-center">
                                <div class="ps-3">
                                <p style="font-weight: 600; color: rgb(1, 30, 68);">
                                    <span>${item.academicYear}</span><br>
                                    <span>${item.programType}</span><br>
                                </p>
                                <div class="mt-3">
                                    <a href="#" class="btn btn-warning btn-sm" data-bs-toggle="modal" data-bs-target="#updateModal" 
                                        data-id="${item.id}" data-name="${item.name}" data-academicyear="${item.academicYear}" data-programtype="${item.programType}">
                                        Update
                                    </a>
                                    <a href="departmentDetails.html?id=${item.id}" class="btn btn-info btn-sm">View Details</a>
                                </div>
                                </div>
                            </div>
                            </div>
                        </div>
                        </div>
                    `;
                    container.innerHTML += cardHtml;
                    });
                }
                const updateModal = document.getElementById('updateModal');
                updateModal.addEventListener('show.bs.modal', event => {
                    const button = event.relatedTarget; // Button that triggered the modal
                    const id = button.getAttribute('data-id');
                    const name = button.getAttribute('data-name');
                    const academicYear = button.getAttribute('data-academicyear');
                    const programType = button.getAttribute('data-programtype');

                    // Update the modal's content
                    const itemIdInput = document.getElementById('itemId');
                    const itemNameInput = document.getElementById('itemName');
                    const academicYearInput = document.getElementById('academicYear');
                    const programTypeInput = document.getElementById('programType');

                    itemIdInput.value = id;
                    itemNameInput.value = name;
                    academicYearInput.value = academicYear;
                    programTypeInput.value = programType;
                });
            }).catch(error => {
        errorMessage.innerText = error;
        errorAlert.classList.remove('d-none');

    })
}

    fetchDepartment();


    
    document.querySelector('.updateDepartment-form').addEventListener('submit', function(event) {
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
        fetch(`http://localhost:5087/admin/api/update/department/details/${id}`, {
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
            }else{
                return response.json().then(data => {
                    // Display error message for incorrect credentials
                    errorMessage.innerText = data.message ||'An error occurred. Please try again later.';
                    errorAlert.classList.remove('d-none');
                })
            }
        })
        

    })




    document.querySelector('.addDepartment-form').addEventListener('submit', function(event) {
        event.preventDefault();
        

        const form = this;
        const formData = new FormData(form);

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


        
        

        fetch('http://localhost:5087/admin/api/create/department/', {
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
            
            if (response.status===201) {
                return response.json().then(data => {
                    document.querySelector('.addDepartment-form').reset();
                    successMessage.innerText = data.message;
                    successAlert.classList.remove('d-none');
                    
                });

            } else {
                return response.json().then(data => {
                // Handle other error statuses
                errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                errorAlert.classList.remove('d-none');
                })
            }
        })
        
    });

    

});