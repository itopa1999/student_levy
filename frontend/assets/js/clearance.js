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

    fetch('http://localhost:5087/student/api/student/clearance', {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
        if (response.ok) {
            return response.json().then(data => {
                document.getElementById('DepartmentName').innerHTML = data.departmentName;
                document.getElementById('academicYearDep').innerHTML = data.academicYear;
                document.getElementById('ProTypeDepartment').innerHTML = data.programType;
                const container = document.getElementById('card-container');
                container.innerHTML =""
                if (data.semesters.$values.length === 0) {
                    container.innerHTML = `
                      <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                      </div>
                    `;
                } else {data.semesters.$values.forEach(item => {
                    const cardHtml = `
                      <div class="col-xxl-4 col-md-4">
                        <div class="card info-card sales-card">
                          <div class="card-body">
                            <h5 class="card-title"><strong>${item.semesterName}</strong></h5>
                            <div class="d-flex align-items-center">
                              <div class="ps-3">
                                <p style="font-weight: 600; color: rgb(1, 30, 68);">
                                    <!-- Show ToBalance value -->
                                    <span style="font-weight:bolder" class="${item.totalToBalance > 0 ? 'text-danger' : 'text-success'}">
                                        ToBalance: ₦${item.totalToBalance.toFixed(2)}
                                    </span><br>
                                </p>

                                <!-- Show the print button only if ToBalance is 0 -->
                                ${item.totalToBalance === 0 ? `
                                    <div class="mt-3">
                                        <button class="btn btn-warning btn-sm print-clearance-btn" data-id="${item.semesterId}">
                                            Print Clearance
                                        </button>
                                    </div>
                                ` : `
                                    <p class="text-primary">Outstanding balance must be cleared before printing clearance.</p>
                                `}
                            </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    `;
                    container.innerHTML += cardHtml;
                })

            }
            document.querySelectorAll('.print-clearance-btn').forEach(button => {
                button.addEventListener('click', async function () {
                    const id = this.getAttribute('data-id');
                    fetch(`http://localhost:5087/student/api/print/clearance/receipt/${id}`, {
                        method: 'GET',
                        headers: {
                            'Authorization': 'Bearer ' + token
                        }
                      })
                      .then(response => {
                        if (response.status === 200) {
                            return response.blob(); // Convert the response to a Blob
                        } else{
                          errorMessage.innerText = 'Semester is unavailable';;
                          errorAlert.classList.remove('d-none');
                        }
                    })
                    .then(blob => {
                        const url = window.URL.createObjectURL(blob); // Create a URL for the Blob
                        const a = document.createElement('a'); // Create an anchor element
                        a.href = url; // Set the href to the Blob URL
                        a.download = `clearance_${id}.pdf`; // Set the desired file name for download
                        document.body.appendChild(a); // Append the anchor to the body
                        a.click(); // Programmatically click the anchor to trigger the download
                        a.remove(); // Remove the anchor from the DOM
                        window.URL.revokeObjectURL(url); // Release the Blob URL
                    })
                    .catch(error => {
                      
                    });
                  })
                })
                  })
                  
                }else {
                  return response.json().then(data => {
                    // Handle other error statuses
                    errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
                    errorAlert.classList.remove('d-none');
                  })
                }
            }).catch(error => {
              errorMessage.innerText = 'Server is not responding. Please try again later.';
              errorAlert.classList.remove('d-none');
            })
        
        })