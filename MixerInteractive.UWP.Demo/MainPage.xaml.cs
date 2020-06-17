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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
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
        private GameClient _gameClient;

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

            _gameClient = new GameClient();
            _gameClient.OpenObs.Subscribe(_ =>
            {
                Debug.WriteLine("GameClient opened!");
            });

            await _gameClient.OpenAsync(new GameClientOptions { AuthToken = _tokens.AccessToken, VersionId = 475488 });

            _gameClient.State.OnParticipantJoin.Subscribe(participant =>
            {
                participants.Add(participant);
            });
            _gameClient.State.OnParticipantLeave.Subscribe(participant =>
            {
                var found = participants.FirstOrDefault(x => x.SessionID == participant.SessionID);
                if (found != null)
                    participants.Remove(found);
            });

            debugText.Text = "Opened";
            var state = await _gameClient.SynchronizeStateAsync();
            debugText.Text = "State Synchronized";

            await _gameClient.ReadyAsync();
            debugText.Text = "Ready";

            await _gameClient.CreateGroupsAsync(CreateGroups());

            await RegisterEventsAsync(state.Item2);

            base.OnNavigatedTo(e);
        }

        private IEnumerable<Group> CreateGroups()
        {
            var pollGroup = new Group { GroupID = "poll", SceneID = "default" };
            var nothingGroup = new Group { GroupID = "nothing", SceneID = "empty" };
            var defaultWithResultsGroup = new Group { GroupID = "defaultWithResults", SceneID = "defaultWithResults" };
            var resultViewGroup = new Group { GroupID = "resultsView", SceneID = "resultsView" };

            var groups = new List<Group>();
            groups.Add(pollGroup);
            groups.Add(nothingGroup);
            groups.Add(defaultWithResultsGroup);
            groups.Add(resultViewGroup);
            return groups;
        }

        private async Task RegisterEventsAsync(IEnumerable<Scene> scenes)
        {
            var pollOnScene = scenes.FirstOrDefault(x => x.SceneID == "default");
            //foreach (var control in pollOnScene.Controls.Values.Where(x=>x.Kind == "button"))
            //{
                
            //}
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var participant in participants)
            {
                if (participant.GroupID == "poll")
                    participant.GroupID = "nothing";
                else
                    participant.GroupID = "poll";
            }
            await _gameClient.UpdateParticipantsAsync(participants);
        }
    }
}
