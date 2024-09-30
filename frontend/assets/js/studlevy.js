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
    fetch(`http://localhost:5087/student/api/get/levies/details`, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
        if (response.ok) {
            return response.json().then(data => {
                document.getElementById('DepartmentName').innerHTML = data.name;
                document.getElementById('academicYearDep').innerHTML = data.academicYear;
                document.getElementById('ProTypeDepartment').innerHTML = data.programType;
                const container = document.getElementById('accordionFlushExample');
                container.innerHTML = ""
                console.log(data.semesters)
                if (data.semesters.$values.length === 0) {
                    const cardHtml1 = `
                      <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                      </div>
                    `;
                    container.innerHTML += cardHtml1;
                    
                }else{
                    data.semesters.$values.forEach(item => {
                    const cardHtml = `
                    <div class="accordion-item">
                            <h2 class="accordion-header" id="flush-heading${item.id}">
                              <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#flush-collapse${item.id}" aria-expanded="false" aria-controls="flush-collapse${item.id}">
                              ${item.name} Levy
                              </button>
                            </h2>
                            <div id="flush-collapse${item.id}" class="accordion-collapse collapse" aria-labelledby="flush-headingOne" data-bs-parent="#accordionFlushExample">
                              <div class="accordion-body">
                              ${item.levies.$values.length > 0 ? item.levies.$values.map(levy => `
                                <li class="list-group-item d-flex justify-content-between align-items-start">
                                    <div class="ms-2 me-auto">
                                        <div class="fw-bold">${levy.name}</div>
                                        <span style="font-size: 0.85rem;">CreatedAt: ${levy.createdAt}</span>
                                        <div class="mt-2"><a href="#" class="btn btn-warning btn-sm" data-bs-toggle="modal" data-bs-target="#stuPayModal" 
                                          data-id="${levy.id}" data-toBalance="${levy.toBalance}" data-name="${levy.name}" data-semester="${levy.semesterName}">
                                          pay
                                        </a>
                                        </div>
                                    </div>
                                    <a href="#!" class="btn btn-secondary btn-sm" style="font-size:0.78em">Amt: ₦${levy.amount.toFixed(2)}</a>
                                    <a href="#!" class="btn btn-success btn-sm" style="font-size:0.78em">TB: ₦${levy.toBalance.toFixed(2)}</a>
                                </li><hr>
                            `).join('') : '<p>No levies available for this semester.</p>'}
                              </div>
                            </div>
                          </div>
                        `;
                        container.innerHTML += cardHtml;
                    });
                
            }
            const updateModal = document.getElementById('stuPayModal');
            updateModal.addEventListener('show.bs.modal', event => {
              const button = event.relatedTarget; // Button that triggered the modal
              const id = button.getAttribute('data-id');
              const name = button.getAttribute('data-name');
              const toBalance = button.getAttribute('data-toBalance');
              const semester = button.getAttribute('data-semester');
              console.log(toBalance)

              
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
                
            })
        }else{
          return response.json().then(data => {
          errorMessage.innerText =data.message || 'unexpected error ocurred. Please try again later.';
          errorAlert.classList.remove('d-none');
          })
        }

    })
    .catch(error => {
      errorMessage.innerText = 'Server is not responding. Please try again later.';
      errorAlert.classList.remove('d-none');
    })





});
