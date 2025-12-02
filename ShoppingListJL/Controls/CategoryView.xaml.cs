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

            if (Resources.TryGetValue("ProductsVM", out var vmObj) &&
                vmObj is ProductsViewModel vm &&
                BindingContext is Category cat)
            {
                vm.Category = cat;
                vm.List = cat.ParentList;
            }
        }

    }
}