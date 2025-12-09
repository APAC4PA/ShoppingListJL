using System.ComponentModel;
using System.Windows.Input;
using ShoppingListJL.Models;
using ShoppingListJL.ViewModels;
using Microsoft.Maui.Controls;

namespace ShoppingListJL.Controls
{
    public partial class CategoryView : ContentView
    {
        public CategoryView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty DeleteCategoryCommandProperty =
            BindableProperty.Create(
                nameof(DeleteCategoryCommand),
                typeof(ICommand),
                typeof(CategoryView),
                default(ICommand));

        public ICommand DeleteCategoryCommand
        {
            get => (ICommand)GetValue(DeleteCategoryCommandProperty);
            set => SetValue(DeleteCategoryCommandProperty, value);
        }
        public static readonly BindableProperty ShowItemsCommandProperty =
            BindableProperty.Create(
                nameof(ShowItemsCommand),
                typeof(ICommand),
                typeof(CategoryView),
                default(ICommand));

        public ICommand ShowItemsCommand
        {
            get => (ICommand)GetValue(ShowItemsCommandProperty);
            set => SetValue(ShowItemsCommandProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (Resources.TryGetValue("ProductsVM", out var vmObj) && // Try to get the "ProductsVM" resource from the control's Resources dictionary
                vmObj is ProductsViewModel vm &&                      // Ensure the resource is of type ProductsViewModel and cast it to vm
                BindingContext is Category cat)                       // Ensure the current BindingContext is a Category and cast it to cat
            {
                vm.Category = cat;            // Assign the current category to the ProductsViewModel
                vm.List = cat.ParentList;     // Assign the parent shopping list so the ViewModel has list context
            }
        }
    }
}