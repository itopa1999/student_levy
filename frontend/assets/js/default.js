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
    fetch('http://localhost:5087/admin/api/defaulting/students', {
      method: 'GET',
      headers: {
          'Authorization': 'Bearer ' + token
      }
    }).then(response => {
        if (response.status===200) {
          return response.json().then(data => {
            console.log(data)
            document.getElementById('defaultAmount').innerHTML = data.totalDefault;
            const tableBody = document.querySelector('.table-container'); // Target table body
            tableBody.innerHTML = ''; // Clear any existing rows

            if (data.defaulting.$values.length === 0) {
            tableBody.innerHTML = `
                <tr>
                <td colspan="7" class="text-center">No data found</td>
                </tr>
            `;
            } else {
            data.defaulting.$values.forEach((levy, index) => {
                const rowHtml = `
                <tr>
                    <th scope="row">${levy.id}</th>
                    <td>${levy.studentFName} ${levy.studentLName}</td>
                    <td>${levy.name}</td>
                    <td>${levy.semesterName}</td>
                    <td>${levy.amount}</td>
                    <td>${levy.toBalance}</td>
                    <td>${new Date(levy.createdAt).toLocaleDateString()}</td>
                </tr>
                `;
                tableBody.innerHTML += rowHtml;
            })
        }
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