namespace WomenBeautyBoutique.Layout
{
    public partial class MainLayout
    {
        private string username = string.Empty;
        private string password = string.Empty;
        private string modalStyle = "display:none;";
        private string errorMessage = string.Empty;
        private async void Login()
        {
            if (username == string.Empty || password == string.Empty)
            {
                errorMessage = "Username and password must not be empty";
                StateHasChanged();
                return;
            }
            if (await adminService.AuthenticateAsync(username, password))
            {
                username = string.Empty;
                password = string.Empty;
                ToggleLogin(false);
                errorMessage = string.Empty;
                StateHasChanged();
            }
            else
            {
                username = string.Empty;
                password = string.Empty;
                errorMessage = "Error occured in Authentication";

                StateHasChanged();
            }
        }
        private void ToggleLogin(bool display)
        {
            if (display)
                modalStyle = "display:block;";
            else
                modalStyle = "display:none;";
        }

    }
}