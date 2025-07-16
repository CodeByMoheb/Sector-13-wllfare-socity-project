
// Donation Success Page JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize donation success page functionality
    initializeDonationSuccess();
});

function initializeDonationSuccess() {
    // Check if we're on the donation success page
    const donationData = document.getElementById('donationData');
    if (!donationData) return;

    // Get donation data from data attributes
    const donationInfo = {
        receiptNumber: donationData.dataset.receiptNumber,
        transactionId: donationData.dataset.transactionId,
        amount: donationData.dataset.amount,
        donationDate: donationData.dataset.donationDate,
        donationTime: donationData.dataset.donationTime,
        donorName: donationData.dataset.donorName,
        donorEmail: donationData.dataset.donorEmail,
        donorPhone: donationData.dataset.donorPhone,
        donationType: donationData.dataset.donationType
    };

    // Add event listeners
    const downloadBtn = document.getElementById('downloadReceiptBtn');
    const printBtn = document.getElementById('printReceiptBtn');

    if (downloadBtn) {
        downloadBtn.addEventListener('click', () => downloadReceipt(donationInfo));
    }

    if (printBtn) {
        printBtn.addEventListener('click', () => printReceipt(donationInfo));
    }

    // Initialize animations and effects
    initializeAnimations();
    createConfetti();
    checkScreenSize();
}

// Responsive design functions
function checkScreenSize() {
    const width = window.innerWidth;
    const actionButtons = document.querySelector('.action-buttons');
    
    if (actionButtons) {
        if (width < 768) {
            actionButtons.classList.add('mobile-layout');
        } else {
            actionButtons.classList.remove('mobile-layout');
        }
    }
}

// Print receipt function
function printReceipt(donationInfo) {
    const receiptContent = generateReceiptHTML(donationInfo, true);
    
    // Open new window with receipt
    const printWindow = window.open('', '_blank');
    printWindow.document.write(receiptContent);
    printWindow.document.close();
    
    // Wait for content to load then print
    printWindow.onload = function() {
        printWindow.print();
        printWindow.close();
    };
}

// Download receipt function
function downloadReceipt(donationInfo) {
    const receiptContent = generateReceiptHTML(donationInfo, false);
    
    // Create blob and download
    const blob = new Blob([receiptContent], { type: 'text/html' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Receipt_${donationInfo.receiptNumber}.html`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}

// Generate receipt HTML content
function generateReceiptHTML(donationInfo, isForPrint) {
    return `
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Receipt - ${donationInfo.receiptNumber}</title>
            <style>
                @media print {
                    body { margin: 0; padding: 10px; }
                    .receipt { box-shadow: none; border: 1px solid #000; }
                }
                
                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }
                
                body {
                    font-family: Arial, sans-serif;
                    background: white;
                    padding: 20px;
                    font-size: 12px;
                    line-height: 1.4;
                }
                
                .receipt {
                    max-width: 360px;
                    margin: 0 auto;
                    background: white;
                    border: 2px solid #333;
                    padding: 15px;
                }
                
                .header {
                    text-align: center;
                    border-bottom: 2px solid #333;
                    padding-bottom: 10px;
                    margin-bottom: 10px;
                }
                
                .logo {
                    font-size: 15px;
                    font-weight: bold;
                    color: #28a745;
                    margin-bottom: 5px;
                }
                
                .org-name {
                    font-size: 16px;
                    font-weight: bold;
                    margin-bottom: 3px;
                }
                
                .receipt-title {
                    font-size: 12px;
                    font-weight: bold;
                    margin-bottom: 5px;
                }
                
                .receipt-number {
                    font-size: 12px;
                    color: #666;
                }
                
                .section {
                    margin-bottom: 15px;
                }
                
                .section-title {
                    font-size: 10px;
                    font-weight: bold;
                    border-bottom: 1px solid #ccc;
                    padding-bottom: 5px;
                    margin-bottom: 5px;
                }
                
                .row {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 4px;
                    font-size: 11px;
                }
                
                .label {
                    font-weight: bold;
                    min-width: 80px;
                }
                
                .value {
                    text-align: right;
                }
                
                .amount {
                    background: #f0f0f0;
                    padding: 8px;
                    text-align: center;
                    margin: 10px 0;
                    border: 1px solid #ccc;
                }
                
                .amount-value {
                    font-size: 16px;
                    font-weight: bold;
                    color: #28a745;
                }
                
                .footer {
                    text-align: center;
                    margin-top: 20px;
                    padding-top: 15px;
                    border-top: 1px solid #ccc;
                    font-size: 10px;
                    color: #666;
                }
                
                .signature {
                    margin-top: 15px;
                    text-align: center;
                }
                
                .signature-line {
                    width: 120px;
                    height: 1px;
                    background: #000;
                    margin: 20px auto 5px;
                    border-bottom: 1px solid #000;
                }
            </style>
        </head>
        <body>
            <div class="receipt">
                <div class="header">
                    <div class="logo"><img src="https://res.cloudinary.com/dmnpdzcvd/image/upload/v1752296638/logo_meh5a8.png" alt="Sector 13" style="width: 100px;" /></div>
                    <div class="org-name">উত্তরা সেক্টর ১৩ <br> ওয়েলফেয়ার  সোসাইটি</div>
                    <div class="receipt-title">DONATION RECEIPT</div>
                    <div class="receipt-number">Receipt #${donationInfo.receiptNumber}</div>
                </div>
                
                <div class="section">
                    <div class="section-title">DONATION DETAILS</div>
                    <div class="row">
                        <span class="label">Date:</span>
                        <span class="value">${donationInfo.donationDate}</span>
                    </div>
                    <div class="row">
                        <span class="label">Time:</span>
                        <span class="value">${donationInfo.donationTime}</span>
                    </div>
                    <div class="row">
                        <span class="label">TXN ID:</span>
                        <span class="value">${donationInfo.transactionId}</span>
                    </div>
                </div>
                
                <div class="amount">
                    <div class="amount-value">৳${donationInfo.amount}</div>
                </div>
                
                <div class="section">
                    <div class="section-title">DONOR INFO</div>
                    <div class="row">
                        <span class="label">Name:</span>
                        <span class="value">${donationInfo.donorName}</span>
                    </div>
                    <div class="row">
                        <span class="label">Email:</span>
                        <span class="value">${donationInfo.donorEmail}</span>
                    </div>
                    <div class="row">
                        <span class="label">Phone:</span>
                        <span class="value">${donationInfo.donorPhone}</span>
                    </div>
                    <div class="row">
                        <span class="label">Type:</span>
                        <span class="value">${donationInfo.donationType}</span>
                    </div>
                </div>
                
                <div class="footer">
                    <div>Thank you for your donation!</div>
                    <div style="margin-bottom: 10px;">This receipt serves as proof of contribution.</div>
                    <div class="signature">
                        <div class="signature-line"></div>
                        <div>Authorized Signature</div>
                    </div>
                </div>
            </div>
        </body>
        </html>
    `;
}

// Social sharing functions
function shareOnFacebook(amount) {
    const url = encodeURIComponent(window.location.href);
    const text = encodeURIComponent(`I just donated ৳${amount} to Sector 13 Welfare Society! Join me in making a difference.`);
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}&quote=${text}`, '_blank');
}

function shareOnTwitter(amount) {
    const url = encodeURIComponent(window.location.href);
    const text = encodeURIComponent(`I just donated ৳${amount} to Sector 13 Welfare Society! Join me in making a difference.`);
    window.open(`https://twitter.com/intent/tweet?url=${url}&text=${text}`, '_blank');
}

function shareOnWhatsApp(amount) {
    const text = encodeURIComponent(`I just donated ৳${amount} to Sector 13 Welfare Society! Join me in making a difference.`);
    window.open(`https://wa.me/?text=${text}`, '_blank');
}

function copyLink() {
    navigator.clipboard.writeText(window.location.href).then(() => {
        // Show success message
        const button = event.target;
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check me-2"></i>Copied!';
        button.classList.remove('btn-outline-secondary');
        button.classList.add('btn-success');
        
        setTimeout(() => {
            button.innerHTML = originalText;
            button.classList.remove('btn-success');
            button.classList.add('btn-outline-secondary');
        }, 2000);
    });
}

// Add confetti animation
function createConfetti() {
    const colors = ['#28a745', '#007bff', '#ffc107', '#dc3545', '#6f42c1'];
    
    for (let i = 0; i < 50; i++) {
        setTimeout(() => {
            const confetti = document.createElement('div');
            confetti.style.position = 'fixed';
            confetti.style.left = Math.random() * 100 + 'vw';
            confetti.style.top = '-10px';
            confetti.style.width = '10px';
            confetti.style.height = '10px';
            confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
            confetti.style.borderRadius = '50%';
            confetti.style.pointerEvents = 'none';
            confetti.style.zIndex = '9999';
            confetti.style.animation = 'fall 3s linear forwards';
            document.body.appendChild(confetti);
            setTimeout(() => {
                confetti.remove();
            }, 3000);
        }, i * 100);
    }
}

// Initialize animations
function initializeAnimations() {
    // Add smooth scroll animation for better UX
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);
    
    // Observe all sections for animation
    document.querySelectorAll('.receipt-details, .donor-details, .impact-section, .next-steps, .social-share').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });
}

// Handle window resize for responsive design
window.addEventListener('resize', checkScreenSize);
