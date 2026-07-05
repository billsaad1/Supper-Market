using System;
using System.Threading.Tasks;
using Supermarket.BLL.Services;
using Supermarket.Models.Entities;

namespace Supermarket.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private string _username;
        private string _password;
        private bool _isBusy;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public async Task<User> LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return null;

            IsBusy = true;
            try
            {
                return await _authService.LoginAsync(Username, Password);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
