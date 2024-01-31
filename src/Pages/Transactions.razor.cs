using WomenBeautyBoutique.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WomenBeautyBoutique.Pages
{
    public partial class Transactions
    {
        private List<InventoryItem> items = new List<InventoryItem>();
        private List<TransactionItem> transactionItems = new List<TransactionItem>();
        private string modalStyle = "display:none;";
        private bool IsAddProduct = true;
        private string searchItem = null;
        private InventoryItem newProduct = new InventoryItem();
        private List<TransactionItem> filteredItems = new List<TransactionItem>();
        protected override async Task OnInitializedAsync()
        {
            items = await JS.InvokeAsync<List<InventoryItem>>("getInventory");
            transactionItems = await JS.InvokeAsync<List<TransactionItem>>("getTransaction");
        }
        private string expandedForecastId = ""; // Empty string indicates no accordion is expanded
        private bool IsSelected = false;
        private void ToggleAccordion(string forecastId)
        {
            if (expandedForecastId == forecastId)
            {
                expandedForecastId = ""; // Collapse the accordion if it's already expanded
            }
            else
            {
                expandedForecastId = forecastId; // Expand the clicked accordion
            }
            StateHasChanged();
        }
        private void EditProduct(InventoryItem item)
        {
            newProduct = new InventoryItem
            {
                Id = item.Id,
                productName = item.productName,
                Brand = item.Brand,
                Quantity = item.Quantity,
                netPrice = item.netPrice,
                sellingPrice = item.sellingPrice
            };
            IsAddProduct = false;
            modalStyle = "display:block;";
        }
        private async void DeleteProduct(InventoryItem item)
        {
            await JS.InvokeVoidAsync("deleteInventory", item.Id);
            ToastService.ShowSuccess("Product removed from inventory!");
            items = await JS.InvokeAsync<List<InventoryItem>>("getInventory");
            StateHasChanged();
        }
        private void AddProduct()
        {
            modalStyle = "display:block;";
            IsAddProduct = true;
        }
        private void CloseAddProductModal()
        {
            modalStyle = "display:none;";
            newProduct = new InventoryItem();
        }
        private async void AddProductToDb()
        {
            var validationErrors = ValidateProduct(newProduct);
            if (newProduct.Quantity == null)
            {
                newProduct.Quantity = 0;
            }

            if (newProduct.netPrice == null)
            {
                newProduct.netPrice = 0;
            }

            if (newProduct.sellingPrice == null)
            {
                newProduct.sellingPrice = 0;
            }
            if (validationErrors.Count == 0)
            {
                await JS.InvokeVoidAsync("addInventory", newProduct);
                ToastService.ShowSuccess("Product added successfully!");
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
        private async void UpdateInventoryItem()
        {
            var validationErrors = ValidateProduct(newProduct);
            if (newProduct.Quantity == null)
            {
                newProduct.Quantity = 0;
            }

            if (newProduct.netPrice == null)
            {
                newProduct.netPrice = 0;
            }

            if (newProduct.sellingPrice == null)
            {
                newProduct.sellingPrice = 0;
            }
            if (validationErrors.Count == 0)
            {
                await JS.InvokeVoidAsync("editInventory", newProduct);
                ToastService.ShowSuccess("Product updated in inventory successfully!");
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
        private void HandleSearchInput(ChangeEventArgs args)
        {
            searchItem = args.Value.ToString();
            UpdateFilteredItems();
        }
        private void UpdateFilteredItems()
        {
            filteredItems = transactionItems
                .Where(product => string.IsNullOrEmpty(searchItem) ||
                                   product.Id.Contains(searchItem, StringComparison.OrdinalIgnoreCase))
                .ToList();
            StateHasChanged();
        }
    }
}
