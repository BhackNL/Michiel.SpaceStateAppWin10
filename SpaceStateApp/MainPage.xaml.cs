using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace SpaceStateApp
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private static readonly Uri SetStateApiUri = new Uri("https://space.bhack.nl/api/slack");
        private static readonly Uri SpaceStateApiUri = new Uri("https://space.bhack.nl/SpaceApi");

        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherTimer _timer;
        private HttpClient _httpClient;

        private SpaceState _spaceState;
        public SpaceState SpaceState
        {
            get { return _spaceState; }
            set
            {
                _spaceState = value;
                NotifyPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;

            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

            _httpClient = new HttpClient(filter);

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 30);
            _timer.Tick += (sender, e) => UpdateSpaceState();
            _timer.Start();

            UpdateSpaceState();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        private async Task UpdateSpaceState()
        {
            _timer.Stop();

            SpaceState = SpaceState.Loading;
            var response = await _httpClient.GetAsync(SpaceStateApiUri);

            if (response.StatusCode != HttpStatusCode.Ok)
                return;

            var stringContent = await response.Content.ReadAsStringAsync();
            var spaceApi = (dynamic)JsonConvert.DeserializeObject(stringContent);

            SpaceState = (bool)spaceApi["state"]["open"].Value ? SpaceState.Open : SpaceState.Closed;

            _timer.Start();
        }


        private async void OpenSpaceClicked(object sender, RoutedEventArgs e)
        {
            await SetSpaceStateAsync("open");
            await UpdateSpaceState();
        }

        private async void CloseSpaceClicked(object sender, RoutedEventArgs e)
        {
            await SetSpaceStateAsync("close");
            await UpdateSpaceState();
        }


        private async void EspressoClicked(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Not implemented!", "Sorry!");

            await messageDialog.ShowAsync();
        }

        private async void LungoClicked(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Not implemented!", "Sorry!");

            await messageDialog.ShowAsync();
        }

        private async Task SetSpaceStateAsync(string state)
        {
            var formValues = new Dictionary<string, string>
            {
                ["text"] = state,
                ["user_name"] = "SpaceStateApp",
                ["token"] = "IPMgcu6fqWX02n9Q5KZDdLVd"
            };

            var content = new HttpFormUrlEncodedContent(formValues.ToList());
            var response = await _httpClient.PostAsync(SetStateApiUri, content);
        }
    }
}
