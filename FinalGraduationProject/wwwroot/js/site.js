// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Cart functionality
document.addEventListener('DOMContentLoaded', function() {
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            if (alert && alert.classList.contains('show')) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });

    // Quantity input validation
    const quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(function(input) {
        input.addEventListener('input', function() {
            const value = parseInt(this.value);
            const min = parseInt(this.getAttribute('min'));
            const max = parseInt(this.getAttribute('max'));

            if (isNaN(value) || value < min) {
                this.value = min;
            } else if (value > max) {
                this.value = max;
                // Show a warning
                const warningToast = document.createElement('div');
                warningToast.className = 'toast align-items-center text-white bg-warning border-0 position-fixed bottom-0 end-0 m-3';
                warningToast.setAttribute('role', 'alert');
                warningToast.innerHTML = `
                    <div class="d-flex">
                        <div class="toast-body">
                            Only ${max} items available in stock!
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                `;
                document.body.appendChild(warningToast);
                const toast = new bootstrap.Toast(warningToast);
                toast.show();
                
                // Remove toast after it's hidden
                warningToast.addEventListener('hidden.bs.toast', function() {
                    this.remove();
                });
            }
        });
    });
});
