using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ide
{
    public partial class SettingsConsoleWindow : Window
    {

        private MainWindow main;

        public ObservableCollection<FontFamily> SystemFonts = new ObservableCollection<FontFamily>();

        private string oldFontFamily;

        private string oldPart;

        private int oldFontSize;

        public void LoadSystemFonts()
        {
            SystemFonts.Clear();
            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());
            foreach (var f in fonts) SystemFonts.Add(f);
            FontFamilySelector.ItemsSource = SystemFonts;
        }

        public SettingsConsoleWindow(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemFonts();

            oldFontFamily = main.settings.NameFontConsole;
            oldFontSize = main.settings.SizeFontConsole;
            oldPart = main.settings.ConsolePath;

            ComboBoxSizeFont.Text = oldFontSize.ToString();
            FontFamilySelector.SelectedValue = new FontFamily(oldFontFamily);
            TextBoxWorkingPart.Text = oldPart;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            main.settings.NameFontConsole = FontFamilySelector.Text;
            main.settings.SizeFontConsole = int.Parse(ComboBoxSizeFont.Text);
            main.settings.ConsolePath = TextBoxWorkingPart.Text;

            main.UpdateConsole();
            Close();
        }
    }
}
