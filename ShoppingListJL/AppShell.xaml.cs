namespace ShoppingListJL
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.CategoryPage), typeof(Views.CategoryPage));
        }
    }
}
