using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ClickMe_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string who = string.IsNullOrEmpty(NameInput.Text) ? "World" : NameInput.Text;
            HelloMessage.Text = $"Hello, {who}!";
        }
    }
}
