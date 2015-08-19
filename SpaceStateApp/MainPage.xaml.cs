using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        private DispatcherTimer _timer;

        private bool _isSpaceOpen;
        public bool IsSpaceOpen
        {
            get { return _isSpaceOpen; }
            set
            {
                _isSpaceOpen = value;
                NotifyPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Tick += (sender, e) => GetSpaceState();
            _timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task GetSpaceState()
        {
            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

            var httpClient = new HttpClient(filter);

            var response = await httpClient.GetAsync(SpaceStateApiUri);

            if (response.StatusCode != HttpStatusCode.Ok)
                return;

            var stringContent = await response.Content.ReadAsStringAsync();
            var content = (dynamic)JsonConvert.DeserializeObject(stringContent);

            IsSpaceOpen = (bool)content["state"]["open"].Value;
        }


        private async void OpenSpaceClicked(object sender, RoutedEventArgs e)
        {
            await SetSpaceStateAsync("open");
        }

        private async void CloseSpaceClicked(object sender, RoutedEventArgs e)
        {
            await SetSpaceStateAsync("close");
        }

        private async Task SetSpaceStateAsync(string state)
        {
            var httpClient = new HttpClient();

            var formValues = new Dictionary<string, string>
            {
                ["text"] = state,
                ["user_name"] = "michiel",
                ["token"] = "IPMgcu6fqWX02n9Q5KZDdLVd"
            };

            var content = new HttpFormUrlEncodedContent(formValues.ToList());
            var response = await httpClient.PostAsync(SetStateApiUri, content);
        }
    }
}
