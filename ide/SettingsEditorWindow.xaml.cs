using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ide
{
    public partial class SettingsEditorWindow : Window
    {

        private MainWindow main;

        public SettingsEditorWindow(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }

        public ObservableCollection<FontFamily> SystemFonts = new ObservableCollection<FontFamily>();

        private string oldFontFamily;
        private int oldFontSize;
        private bool oldShowLineNumbers;

        public void LoadSystemFonts()
        {
            SystemFonts.Clear();
            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());
            foreach (var f in fonts) SystemFonts.Add(f);
            FontFamilySelector.ItemsSource = SystemFonts;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemFonts();
            oldFontFamily = main.settings.NameFontEditor;
            oldFontSize = main.settings.SizeFontEditor;
            oldShowLineNumbers = main.settings.ShowLineNumbers;

            ComboBoxSizeFont.Text = oldFontSize.ToString();
            CheckBoxNum.IsChecked = oldShowLineNumbers;
            FontFamilySelector.SelectedValue = new FontFamily(oldFontFamily);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            main.settings.NameFontEditor = oldFontFamily;
            main.settings.SizeFontEditor = oldFontSize;
            main.settings.ShowLineNumbers = oldShowLineNumbers;

            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            main.settings.NameFontEditor = FontFamilySelector.Text;
            main.settings.SizeFontEditor = int.Parse(ComboBoxSizeFont.Text);
            main.settings.ShowLineNumbers = (bool)CheckBoxNum.IsChecked;

            main.UpdateEdit();
            Close();
        }
    }
}
