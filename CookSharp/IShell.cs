namespace CookSharp
{
    public interface IShell
    {
        void Run();
    }

    public class Shell : IShell
    {
        private readonly MainWindow _mainWindow;

        public Shell(MainWindow window)
        {
            _mainWindow = window;
        }

        public void Run()
        {
            _mainWindow.Show();
        }
    }
}