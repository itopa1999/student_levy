document.addEventListener('DOMContentLoaded', function() {

    const token = localStorage.getItem('levy_token')
    const Id = new URLSearchParams(window.location.search).get('id');

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
                                    </div>
                                    <a href="#!" class="btn btn-secondary btn-sm" style="font-size:0.78em">Amt: ${levy.amount}</a>
                                    <a href="#!" class="btn btn-success btn-sm" style="font-size:0.78em">TB: ${levy.toBalance}</a>
                                </li><hr>
                            `).join('') : '<p>No levies available for this semester.</p>'}
                              </div>
                            </div>
                          </div>
                        `;
                        container.innerHTML += cardHtml;
                    });
                
            }
                
            })
        }

    })





});
