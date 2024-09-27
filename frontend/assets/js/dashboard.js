document.addEventListener('DOMContentLoaded', function() {
    const token = localStorage.getItem('levy_token')
    if (token == null && token == 'undefined') {
      window.location.href = 'login.html';
    }
    const errorAlert = document.getElementById('error-alert');
    const errorMessage = document.getElementById('error-message');
    const successAlert = document.getElementById('success-alert');
    const successMessage = document.getElementById('success-message');

    // Reset previous messages
    errorAlert.classList.add('d-none');
    errorMessage.innerHTML = '';
    successAlert.classList.add('d-none');
    successMessage.innerHTML = '';
    
    const t_department = document.getElementById('t_department');
    const t_student = document.getElementById('t_student');
    fetch('http://localhost:5087/admin/api/admin/dashboard', {
      method: 'GET',
      headers: {
          'Authorization': 'Bearer ' + token
      }
    }).then(response => {
        if (response.ok) {
          return response.json().then(data => {
            console.log(data.transactions.$values)
            t_department.innerHTML = data.t_department;
            t_student.innerHTML = data.t_student;
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
                    <td>${transaction.studentFName} ${transaction.studentLName}</td>
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
          })
        }else {
          // Handle other error statuses
          errorMessage.innerText = 'An error occurred. Please try again later.';
          errorAlert.classList.remove('d-none');
        }
    })

})