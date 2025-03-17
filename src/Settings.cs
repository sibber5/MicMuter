using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace MicMuter
{
    public class Settings : INotifyPropertyChanged
    {
        public string InputDeviceID { get; set; }

        public int HotkeyKey { get; set; } = 96;
        public int HotkeyModifiers { get; set; } = 0;

        private bool _muteMicHotkeyIsEnabled = false;
        public bool MuteMicHotkeyIsEnabled
        {
            get { return _muteMicHotkeyIsEnabled; }
            set
            {
                _muteMicHotkeyIsEnabled = value;
                NotifyPropertyChanged();
            }
        }
        private bool _startWithWindows = false;
        public bool StartWithWindows
        {
            get { return _startWithWindows; }
            set
            {
                _startWithWindows = value;
                NotifyPropertyChanged();
            }
        }
        [System.Xml.Serialization.XmlAnyElement("ReqAdminComment")]
        public XmlComment ReqAdminComment { get { return new XmlDocument().CreateComment("WARNING: Changing this setting from here might not work as expected."); } set { } }
        private bool _requireAdmin = false;
        public bool RequireAdmin
        {
            get { return _requireAdmin; }
            set
            {
                _requireAdmin = value;
                NotifyPropertyChanged();
            }
        }
        private bool _startMinimized = false;
        public bool StartMinimized
        {
            get { return _startMinimized; }
            set
            {
                _startMinimized = value;
                NotifyPropertyChanged();
            }
        }
        private bool _notifyRunning = true;
        public bool NotifyRunning
        {
            get { return _notifyRunning; }
            set
            {
                _notifyRunning = value;
                NotifyPropertyChanged();
            }
        }

        public Settings()
        {
            InputDeviceID = App.SelectedInputDevice.ID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
