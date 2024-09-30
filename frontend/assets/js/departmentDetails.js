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
      currentPage = 1;
      fetchDepartmentDetails();
    });
    function fetchDepartmentDetails() {

      const searchInput = document.getElementById('searchInput').value;
      const url = `http://localhost:5087/admin/api/get/department/details/${Id}?FilterOptions=${searchInput}`;

    fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
    .then(response => {
      if (response.ok) {
          return response.json();
      } else {
        return response.json().then(data => {
            errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
          })
      }
    })
    .then(data => {
                document.getElementById('departmentName').innerHTML = data.name;
                document.getElementById('departmentAcademyYear').innerHTML = data.academicYear;
                document.getElementById('departmentProgramType').innerHTML = data.programType;
                document.getElementById('t_students').innerHTML = data.students.$values.length;
                document.getElementById('t_semester').innerHTML = data.semesters.$values.length;
                
                const container = document.getElementById('semester-container');
                const stu_container = document.getElementById('student-container');
                container.innerHTML =""
                stu_container.innerHTML =""
                if (data.semesters.$values.length === 0) {
                  const cardHtml = `
                      <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                      </div>
                    `;
                    container.innerHTML += cardHtml;
                }else{
                    {data.semesters.$values.forEach(item => {
                    const cardHtml = `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${item.name}
                      <a href="levyDetails.html?id=${item.id}" class="btn btn-info btn-sm">View Levies</a>
                    </li>
                        `;
                        container.innerHTML += cardHtml;
                    });
                }
                }


                if (data.students.$values.length === 0) {
                  const cardHtml = `
                      <div class="d-flex justify-content-center align-items-center" style="height: 200px;">
                        <h5>No data found</h5>
                      </div>
                    `;
                    stu_container.innerHTML += cardHtml;
                }else{
                    {data.students.$values.forEach(item => {
                    const cardHtml = `
                    <li class="list-group-item d-flex justify-content-between align-items-start">
                      <div class="ms-2 me-auto">
                        <div class="fw-bold">${item.firstName} ${item.lastName}</div>
                        ${item.matricNo}
                      </div>
                      <a href="studentDetails.html?id=${item.id}" class="btn btn-info btn-sm">View Details</a>
                    </li>
                        `;
                        stu_container.innerHTML += cardHtml;
                    });
                }
              }
                
    }).catch(error => {
      errorMessage.innerText = 'Server is not responding. Please try again later.';
      errorAlert.classList.remove('d-none');
    })

  }
  fetchDepartmentDetails();

})