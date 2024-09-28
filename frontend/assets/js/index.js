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

    fetch('http://localhost:5087/student/api/student/dashboard', {
      method: 'GET',
      headers: {
          'Authorization': 'Bearer ' + token
      }
    }).then(response => {
        if (response.status===200) {
          return response.json().then(data => {
            document.getElementById('stuBalance').innerHTML = data.stu_balance;
            const tableBody = document.querySelector('.table-container'); // Target table body
            tableBody.innerHTML = ''; // Clear any existing rows
            console.log(data)
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
    .catch(error => {
      errorMessage.innerText = 'Server is not responding. Please try again later.';
      errorAlert.classList.remove('d-none');
    })

})