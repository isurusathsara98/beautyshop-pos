
function printBill(content) {
    var printWindow = window.open('', '_blank');
    printWindow.document.open();
    printWindow.document.write(content); // Insert your bill content here
    printWindow.document.close();
    printWindow.print();
}
