using WomenBeautyBoutique.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WomenBeautyBoutique.Pages
{
    public partial class ProductInventory
    {
        private List<InventoryItem> items = new List<InventoryItem>();
        private List<TransactionItem> transactionItem = new List<TransactionItem>();
        private string modalStyle = "display:none;";
        private bool IsAddProduct = true;
        private string searchItem = null;
        private InventoryItem newProduct = new InventoryItem();
        private List<InventoryItem> filteredItems = new List<InventoryItem>();
        protected override async Task OnInitializedAsync()
        {
            items = await JS.InvokeAsync<List<InventoryItem>>("getInventory");
            adminService.OnAuthenticationChanged += HandleAuthenticationChanged;
        }

        private void HandleAuthenticationChanged()
        {
            StateHasChanged();
        }
        public void Dispose()
        {
            // Unsubscribe from the OnAuthenticationChanged event
            adminService.OnAuthenticationChanged -= HandleAuthenticationChanged;
        }

        private void EditProduct(InventoryItem item)
        {
            newProduct = new InventoryItem
            {
                Id = item.Id,
                productId = item.productId,
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
            filteredItems = items
                .Where(product => string.IsNullOrEmpty(searchItem) ||
                                   product.productName.Contains(searchItem, StringComparison.OrdinalIgnoreCase) ||
                                   product.Brand.Contains(searchItem, StringComparison.OrdinalIgnoreCase))
                .ToList();
            StateHasChanged();
        }
    }
}
