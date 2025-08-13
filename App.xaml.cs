using Restaurant_Manager.Data;

namespace Restaurant_Manager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();


          
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Restaurant_Manager" };
        }
    }
}
