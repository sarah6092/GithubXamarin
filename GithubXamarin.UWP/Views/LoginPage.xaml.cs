﻿using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GithubXamarin.UWP.Services;
using Octokit;
using Template10.Services.NavigationService;
using Page = Windows.UI.Xaml.Controls.Page;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GithubXamarin.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private GitHubClient _client;
        private string _clientId;
        private string _clientSecret;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Values can be found at https://github.com/settings/applications
                _clientId = "5c0821cdb943e8e2fc0c";
                _clientSecret = "e8e49568f6466fa7039ce49cb493f4aa35efec1d";
                _client = new GitHubClient(new ProductHeaderValue("githubuwp"));

                var loginRequest = new OauthLoginRequest(_clientId)
                {
                    Scopes = { "user", "notifications", "repo", "gist", "read:org" }
                };

                var oAuthLoginUrl = _client.Oauth.GetGitHubLoginUrl(loginRequest);
                LoginWebView.Navigate(oAuthLoginUrl);
                LoginWebView.NavigationCompleted += LoginWebViewOnNavigationCompleted;
            
        }

        /// <summary>
        /// Method attached to navigation of the web-browser. It must return void
        /// TODO: async and void don't go well together. Should be Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void LoginWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri  != null && args.Uri.ToString().Contains("code="))
            {
                await CodeRetrieverandTokenSaver(args.Uri.ToString());
            }
        }
        
        

        /// <summary>
        /// Retrieves code value from a given string and uses it to create access token and then saves it.
        /// </summary>
        /// <param name="retrievedUrl">Url from browser</param>
        /// <returns></returns>
        private async Task CodeRetrieverandTokenSaver(string retrievedUrl)
        {
            //Retrieves code from URL
            var code = retrievedUrl.Split(new[] {"code="}, StringSplitOptions.None)[1];
            var tokenRequest = new OauthTokenRequest(_clientId, _clientSecret, code);
            var accessToken = await _client.Oauth.CreateAccessToken(tokenRequest);
            await HelpingWorker.VaultAccessTokenAdder(accessToken);

            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn"))
                ApplicationData.Current.RoamingSettings.Values.Add("IsLoggedIn",true);

            var shell = new Shell();
            await shell.LoginProfileUpdater();
            await NavigationService.GetForFrame(Frame).NavigateAsync(typeof(Views.UserProfilePage));
        }
    }
}
