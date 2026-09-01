using System.ComponentModel;
using System.Globalization;

namespace IdSharp.Tagging.ID3v2.Frames.Items;

internal sealed class LanguageItem : ILanguageItem
{
    private string _languageCode;
    private string _languageDisplay;

    public event PropertyChangedEventHandler PropertyChanged;

    public string LanguageCode
    {
        get => _languageCode;
        set
        {
            _languageCode = value;

            string languageDisplay;
            if (LanguageHelper.Languages.TryGetValue(_languageCode.ToLower(CultureInfo.InvariantCulture), out languageDisplay))
            {
                LanguageDisplay = languageDisplay;
            }
            else
            {
                LanguageDisplay = _languageCode;
                // TODO: notify about bad data?
            }

            RaisePropertyChanged(nameof(LanguageCode));
        }
    }

    public string LanguageDisplay
    {
        get => _languageDisplay;
        private set
        {
            _languageDisplay = value;
            RaisePropertyChanged(nameof(LanguageDisplay));
        }
    }

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
