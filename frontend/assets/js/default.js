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

    let currentPage = 1;

      document.getElementById('searchInput').addEventListener('input', function() {
        currentPage = 1;
        fetchDefaultStudents();
      });

      document.getElementById('prevPage').addEventListener('click', function(e) {
          e.preventDefault();
          if (currentPage > 1) {
              currentPage--;
              fetchDefaultStudents();
          }
      });

      document.getElementById('nextPage').addEventListener('click', function(e) {
          e.preventDefault();
          currentPage++;
          fetchDefaultStudents();
      });

      function fetchDefaultStudents() {

    const searchInput = document.getElementById('searchInput').value;
    const url = `http://localhost:5087/admin/api/defaulting/students?FilterOptions=${searchInput}&PageNumber=${currentPage}`
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
            // Handle other error statuses
            errorMessage.innerText =data.message || 'An error occurred. Please try again later.';
            errorAlert.classList.remove('d-none');
          })
      }
  }).then(data => {
            document.getElementById('defaultAmount').innerHTML = "₦"+data.totalDefault.toFixed(2);
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
                    <td>₦${levy.amount.toFixed(2)}</td>
                    <td>₦${levy.toBalance.toFixed(2)}</td>
                    <td>${new Date(levy.createdAt).toLocaleDateString()}</td>
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
fetchDefaultStudents();

document.getElementById('downloadDefaultStudent').addEventListener('click', function() {
  fetch(`http://localhost:5087/admin/api/download/default/students`, {
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
    a.download = `DefaultStudent.csv`; // Set the desired file name for download
    document.body.appendChild(a); // Append the anchor to the body
    a.click(); // Programmatically click the anchor to trigger the download
    a.remove(); // Remove the anchor from the DOM
    window.URL.revokeObjectURL(url); // Release the Blob URL
})
.catch(error => {
  errorMessage.innerText = 'An error occurred. Please try again later.';
  errorAlert.classList.remove('d-none');
});
})

})