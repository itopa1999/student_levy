
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
      document.getElementById('orderingSelect').addEventListener('change', function() {
        currentPage = 1;
        fetchTransactions();
      });

      document.getElementById('searchInput').addEventListener('input', function() {
        currentPage = 1;
        fetchTransactions();
      });

      document.getElementById('prevPage').addEventListener('click', function(e) {
          e.preventDefault();
          if (currentPage > 1) {
              currentPage--;
              fetchTransactions();
          }
      });

      document.getElementById('nextPage').addEventListener('click', function(e) {
          e.preventDefault();
          currentPage++;
          fetchTransactions();
      });

      function fetchTransactions() {

          const searchInput = document.getElementById('searchInput').value;

          const orderSelect = document.getElementById('orderingSelect').value;

          const url = `http://localhost:5087/student/api/student/transaction?FilterOptions=${searchInput}&OrderOptions=${orderSelect}&PageNumber=${currentPage}`;

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
          })
          .then(data => {
              // Display data in the table
              document.getElementById('t_amount').innerHTML ="₦"+ data.totalPay.toFixed(2);
              document.getElementById('t_bill').innerHTML = "₦"+ data.totalBilling.toFixed(2);
              document.getElementById('t_tobal').innerHTML ="₦"+ data.totalToBal.toFixed(2);

              const tableBody = document.querySelector('.table-container');
              tableBody.innerHTML = ''; // Clear any existing rows

              if (data.transactions.$values.length === 0) {
                  tableBody.innerHTML = `
                      <tr>
                          <td colspan="7" class="text-center">No data found</td>
                      </tr>
                  `;
              } else {
                  data.transactions.$values.forEach((transaction) => {
                      const rowHtml = `
                          <tr>
                              <th scope="row">${transaction.id}</th>
                              <td>${transaction.levyName}</td>
                              <td>₦${transaction.amount.toFixed(2)}</td>
                              <td>${transaction.description}</td>
                              <td>${transaction.method}</td>
                              <td>${new Date(transaction.createdAt).toLocaleString()}</td>
                              <td>${transaction.transID}</td>
                              <td><button data-id="${transaction.id}" class="btn btn-success btn-sm print-receipt-btn" style="font-size:0.75em">Print Receipt</button></td>
                          </tr>
                      `;
                      tableBody.innerHTML += rowHtml;
                  });
              }

              document.querySelectorAll('.print-receipt-btn').forEach(button => {
                button.addEventListener('click', async function () {
                    const id = this.getAttribute('data-id');
                    fetch(`http://localhost:5087/student/api/print/receipt/${id}`, {
                      method: 'GET',
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
                      a.download = `receipt_${id}.pdf`; // Set the desired file name for download
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
          })
          .catch(error => {
            errorMessage.innerText = 'Server is not responding. Please try again later.';
            errorAlert.classList.remove('d-none');
          });
      }

      fetchTransactions();
      
    })


