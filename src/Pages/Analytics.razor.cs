using WomenBeautyBoutique.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WomenBeautyBoutique.Pages
{
    public partial class Analytics
    {
        private List<TransactionItem> transactionItems = new List<TransactionItem>();
        private DateTime SelectedDateTimeFrom { get; set; }
        private DateTime SelectedDateTimeTo { get; set; }

        private int selectedButtonIndex = -1;
        protected override async Task OnInitializedAsync()
        {
            transactionItems = await JS.InvokeAsync<List<TransactionItem>>("getTransaction");
            foreach (var transactionItem in transactionItems)
            {

            }
        }
        private string HighlightButton(int buttonIndex)
        {
            return selectedButtonIndex == buttonIndex ? "background-color: #218838 !important;" : "";
        }
        private async void SetLastDay()
        {
            ToastService.ShowInfo("Set Date Time for Last Day!");
            selectedButtonIndex = 0;
            StateHasChanged();
        }
        private async void SetLastWeek()
        {
            ToastService.ShowInfo("Set Date Time for Last Week!");
            selectedButtonIndex = 1;
            StateHasChanged();
        }
        private async void SetLastMonth()
        {
            ToastService.ShowInfo("Set Date Time for Last Month!");
            selectedButtonIndex = 2;
            StateHasChanged();
        }
    }
}
