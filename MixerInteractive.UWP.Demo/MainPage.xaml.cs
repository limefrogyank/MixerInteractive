using Microsoft.Mixer.ShortcodeOAuth;
using MixerInteractive.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MixerInteractive.UWP.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private OAuthTokens _tokens;

        private ObservableCollection<Participant> participants = new ObservableCollection<Participant>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            participantList.ItemsSource = participants;

            PasswordVault vault = new PasswordVault();

            PasswordCredential credential = null;
            try
            {
                credential = vault.Retrieve("mixer.com", "MixerInteractive.UWP.Demo");
            }
            catch
            {

            }
            
            

            if (credential == null)
            {

                debugText.Text = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri;
                Debug.WriteLine(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri);

                var client = new OAuthClient(
                new OAuthOptions
                {
                    ClientId = "17a72898b684d1f99652476c0a959638d2b735cdfad9e843",
                    Scopes = new[] { "interactive:robot:self" },
                });

                // Use the helper GrantAsync to get codes. Alternately, you can run
                // the granting/polling loop manually using client.GetSingleCodeAsync.
                _tokens = await client.GrantAsync(
                    code =>
                    {
                        Debug.WriteLine($"Go to mixer.com/go and enter {code}");
                        var dialog = new MessageDialog($"Go to mixer.com/go and enter {code}", code);
                        _ = dialog.ShowAsync();
                    },
                    CancellationToken.None);
                

                vault.Add(new PasswordCredential("mixer.com", "MixerInteractive.UWP.Demo", Newtonsoft.Json.JsonConvert.SerializeObject(_tokens)));
            }
            else
            {
                credential.RetrievePassword();
                _tokens = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthTokens>(credential.Password);
            }
            //var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri($"https://mixer.com/oauth/authorize?response_type=token&redirect_uri={WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri}&scope=interactive:robot:self&client_id=17a72898b684d1f99652476c0a959638d2b735cdfad9e843"),WebAuthenticationBroker.GetCurrentApplicationCallbackUri());

            var gameClient = new GameClient();
            gameClient.OpenObs.Subscribe(_ =>
            {
                Debug.WriteLine("GameClient opened!");
            });

            await gameClient.OpenAsync(new GameClientOptions { AuthToken = _tokens.AccessToken, VersionId = 475089 });

            gameClient.State.OnParticipantJoin.Subscribe(participant =>
            {
                participants.Add(participant);
            });
            gameClient.State.OnParticipantLeave.Subscribe(participant =>
            {
                participants.Remove(participant);
            });

            debugText.Text = "Opened";
            await gameClient.ReadyAsync();
            debugText.Text = "Ready";

            base.OnNavigatedTo(e);
        }
    }
}
