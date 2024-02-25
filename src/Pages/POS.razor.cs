using WomenBeautyBoutique.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Transactions;


namespace WomenBeautyBoutique.Pages
{
    public partial class POS
    {

        private List<InventoryItem> items = new List<InventoryItem>();
        private List<InventoryItem> checkList = new List<InventoryItem>();

        private List<InventoryItem> productlist = new List<InventoryItem>();
        private InventoryItem CurrentProduct = new InventoryItem();
        private string searchItem = null;
        private List<InventoryItem> filteredItems = new List<InventoryItem>();
        private TransactionItem newTransaction = new TransactionItem();

        private List<PrintItems> itemsWithPrices = new List<PrintItems>();
        private int quantity = 1;

        private bool Isadding = false; 
        private bool IsmodalOpen = false;
        private bool Ischeckoutavailable = false;

        private int? TotalAmount = 0;
        private int? PaidAmount = 0;
        private int? Discount = 0;
        private int? Status = 0;
        private int? ReturnedAmount = 0;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        private string BillBody = "Sample bill Body";
        protected override async Task OnInitializedAsync()
        {
            TotalAmount = 0;
            items = await JS.InvokeAsync<List<InventoryItem>>("getInventory");
        }

        private async Task AddProductToCheckList()
        {
            CurrentProduct = filteredItems[0];
            CurrentProduct.BuyingQuantity = quantity;
            CurrentProduct.TotalPrice = (CurrentProduct.sellingPrice * quantity);
            TotalAmount += CurrentProduct.TotalPrice;
            checkList.Add(CurrentProduct);
            filteredItems.Clear();
            
        }

        private async Task CheckOut()
        {
            TotalAmount = 0;
            foreach (var list in checkList)
            {
                TotalAmount += list.TotalPrice;
                UpdateProduct(list);
            }

            TotalAmount -= Discount;

            ToastService.ShowSuccess("Product updated in inventory successfully!");

            AddTransactionToDb(newTransaction, checkList);

            ToastService.ShowSuccess("Product updated in inventory successfully!");
            searchItem = null;
            filteredItems.Clear();
            productlist.Clear();
            checkList.Clear();
            itemsWithPrices.Clear();
            IsmodalOpen = false;
        }

        private async Task CalculateReturn()
        {
            ReturnedAmount = PaidAmount - TotalAmount + Discount;
            Ischeckoutavailable = true;
        }

        private async Task PreCheckOut()
        {
            IsmodalOpen = true;
        }

        private async Task CancelCheckout()
        {
            IsmodalOpen = false;
            Ischeckoutavailable = false;
            PaidAmount = 0;
            Discount = 0;
            ReturnedAmount = 0;

        }

        private async Task CloseAddProductModal()
        {
            IsmodalOpen = false;
        }

        private void UpdateProduct(InventoryItem item)
        {
            var newProduct = new InventoryItem
            {
                Id = item.Id,
                productId = item.productId,
                productName = item.productName,
                Brand = item.Brand,
                Quantity = item.Quantity - item.BuyingQuantity,
                netPrice = item.netPrice,
                sellingPrice = item.sellingPrice
            };

            UpdateInventoryItem(newProduct);
        }

        private async void UpdateInventoryItem(InventoryItem item)
        {
            var validationErrors = ValidateProduct(item);
            if (item.Quantity == null)
            {
                item.Quantity = 0;
            }

            if (item.netPrice == null)
            {
                item.netPrice = 0;
            }

            if (item.sellingPrice == null)
            {
                item.sellingPrice = 0;
            }
            if (validationErrors.Count == 0)
            {
                await JS.InvokeVoidAsync("editInventory", item);
                items = await JS.InvokeAsync<List<InventoryItem>>("getInventory");
                CloseAddProductModal();
                StateHasChanged();
            }
            else
            {
                foreach (var error in validationErrors)
                {
                    ToastService.ShowError(error);
                }
            }
        }

        List<string> ValidateProduct(InventoryItem product)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(product.productName))
            {
                validationErrors.Add("Product Name is required.");
            }

            if (string.IsNullOrWhiteSpace(product.Brand))
            {
                validationErrors.Add("Brand is required.");
            }

            return validationErrors;
        }

        private void HandleSearchInput(ChangeEventArgs args)
        {
            searchItem = args.Value.ToString();
            UpdateFilteredItems();
        }
        private void UpdateFilteredItems()
        {
            filteredItems = items
                .Where(product => string.IsNullOrEmpty(searchItem) ||
                                   product.productName.Contains(searchItem, StringComparison.OrdinalIgnoreCase) ||
                                   product.productId.Contains(searchItem, StringComparison.OrdinalIgnoreCase))
                .ToList();
            StateHasChanged();
        }

        private async Task Printbill(string billContent)
        {
            try
            {
                
                foreach (var item in checkList)
                {
                    PrintItems obj = new PrintItems();
                    obj.itemname = item.productName;
                    obj.quantity = item.BuyingQuantity;
                    obj.price = item.sellingPrice * item.BuyingQuantity;
                    itemsWithPrices.Add(obj);
                }
                string billC2ontent = GenerateBillContent(itemsWithPrices);
                await JSRuntime.InvokeVoidAsync("printBill", billC2ontent);
                CheckOut();
            }
            catch (JSDisconnectedException ex)
            {
                // Ignore
            }
        }

        private string GenerateBillContent(List<PrintItems> itemWithPrice)
        {
            // Create table rows for each item in the checklist
            string tableRows = string.Join("\n", checkList.Select(itemWithPrice => $@"
            <tr>
                <td>{itemWithPrice.productName}</td>
                <td>{itemWithPrice.BuyingQuantity}</td>
                <td>Rs. {itemWithPrice.sellingPrice}</td>
            </tr>"));

            // Format your bill content in HTML with a table
            string billContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>womans</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 10px;
                        font-size: 12px; /* Reduced font size */
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 20px;
                    }}
                    .table-container {{
                        margin-bottom: 20px;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                    }}
                    th, td {{
                        padding: 8px;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                    th, td:not(:last-child) {{
                        border-right: none;
                    }}
                    th:first-child, td:first-child {{
                        border-left: none;
                    }}
                    th, td {{
                        border-top: none;
                        border-bottom: none;
                    }}
                    .total {{
                        font-weight: bold;
                    }}
                    .logo {{
                        width: 100px;
                        height: 100px;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        font-style: italic;
                    }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h3>Womans - Beauty Boutique</h3>
                    <!-- Replace '..\logo.png' with your image path -->
                    <img class='logo' src='..\logo.png' alt='Shop Logo'>
                </div>
                <div class='table-container'>
                    <table>
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th>Quantity</th>
                                <th>Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tableRows}
                        </tbody>
                    </table>
                </div>
                <div class='total'>
                    <p>Total: Rs. {TotalAmount}</p>
                    <p>Returned Amount: Rs. {ReturnedAmount}</p>
                </div>
                <div class='footer'>
                    <p>Have a nice day!</p>
                </div>
            </body>
            </html>";

            return billContent;
        }
        List<string> ValidateTransaction(TransactionItem TransactionItem)
        {
            List<string> validationErrors = new List<string>();

            return validationErrors;
        }

        private async void AddTransactionToDb(TransactionItem newTransaction, List<InventoryItem> checkList)
        {
            var validationErrors = ValidateTransaction(newTransaction);
            if (newTransaction.Status == null)
            {
                newTransaction.Status = 0;
            }

            if (newTransaction.TotalAmount == null)
            {
                newTransaction.TotalAmount = 0;
            }
            if (validationErrors.Count == 0)
            {
                TransactionItem transactionData = new TransactionItem();
                transactionData.Status = Status;
                transactionData.Discount = Discount;
                transactionData.TotalAmount = TotalAmount;
                transactionData.productItems = checkList;

                foreach (var list in transactionData.productItems)
                {
                    list.Quantity = list.BuyingQuantity;
                }


                await JS.InvokeVoidAsync("insertTransaction", transactionData);
                ToastService.ShowSuccess("Transaction added successfully!");
                StateHasChanged();

            }
            else
            {
                foreach (var error in validationErrors)
                {
                    ToastService.ShowError(error);
                }
            }
        }

    }
}
