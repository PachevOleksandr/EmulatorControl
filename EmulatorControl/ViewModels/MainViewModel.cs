using EmulatorControl.AndroidCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EmulatorControl.Helpers;
using System.Collections.ObjectModel;
using EmulatorControl.Helpers.Google;

namespace EmulatorControl.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        const string ipSearch = "my ip address";

        private ADBCommandsSender adbSender;
        private GoogleActions? chrome;
        private Logger logger;

        public RelayCommand KillOpenAppsCommand { get; }
        public RelayCommand OpenGhromeCommand { get; }
        public RelayCommand SearchCommand { get; }

        private string googleSearch;
        public string GoogleSearch
        {
            get => googleSearch;
            set
            {
                if (GoogleSearch != value)
                {
                    googleSearch = value.Trim();
                    OnPropertyChanged();
                }
            }
        }

        public List<string> Devices { get; set; }

        private string? activeDevice;

        public string? ActiveDevice
        {
            get => activeDevice;
            set
            {
                if (activeDevice != value)
                {
                    activeDevice = value;
                    ADBCommandsSender.Device = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<string> Logs { get; set; }

        public MainViewModel()
        {
            Logs = new ObservableCollection<string>();
            logger = Logger.Create(Logs);

            adbSender = new ADBCommandsSender();
            chrome = null;

            KillOpenAppsCommand = new RelayCommand(KillOpenApps);
            OpenGhromeCommand = new RelayCommand(OpenChrome);
            SearchCommand = new RelayCommand(Search);

            Devices = ADBCommandsSender.GetDevices().ToList();
            ActiveDevice = Devices.FirstOrDefault();

            GoogleSearch = ipSearch;
        }

        private void OpenChrome(object? obj)
        {
            try
            {
                APILVL api = ADBCommandsSender.GetCurrentDeviceApiLvl();
                chrome = GoogleActionsFactory.Create(api, adbSender);

                if (chrome == null)
                {
                    throw new Exception("Current device has not supported API level. (Please use API_30 or API_34)");
                }

                chrome.Open();
                logger.Log("Chrome is ready to work");
            }
            catch (Exception ex)
            {
                logger.Log($"ERROR: {ex.Message}");
            }
        }

        private void Search(object? obj)
        {
            try
            {
                OpenChrome(obj);
                chrome!.Search(GoogleSearch);

                // Getting ip
                if (GoogleSearch.ToLower() == ipSearch)
                {
                    string ip = chrome.GetIp();
                    logger.Log($"Your public ip is {ip}");
                }
                logger.Log("Success searching");
            }
            catch (Exception ex)
            {
                logger.Log($"ERROR: {ex.Message}");
            }
        }

        private void KillOpenApps(object? obj)
        {
            try
            {
                var apps = adbSender.GetOpenApps();
                foreach (var app in apps)
                {
                    adbSender.KillApp(app);
                }

                logger.Log("Closed all apps successfuly");
            }
            catch (Exception ex)
            {
                logger.Log($"ERROR: {ex.Message}");
            }
        }
    }
}
