using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using ShoppingListJL.Models;

namespace ShoppingListJL.ViewModels
{
    internal class CategoriesViewModel : ObservableObject, IQueryAttributable
    {
        private Models.Category _category;
        private ShoppingList _shoppingList;

        public ShoppingList ShoppingList
        {
            get => _shoppingList;
            set
            {
                _shoppingList = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Categories));
            }
        }
        public Category Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ObservableCollection<Category> Categories => ShoppingList.Categories;
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand NavigateToStoresCommand { get; }

        public string CategoryName
        {
            get => _category.Name;
            set
            {
                if (_category.Name != value)
                {
                    _category.Name = value;
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        public string Identifier => _category.Name;

        public CategoriesViewModel()
        {
            ShoppingList = new ShoppingList();
            _category = new Category();
            AddCategoryCommand = new AsyncRelayCommand(AddCategory);
            DeleteCategoryCommand = new AsyncRelayCommand<Category>(DeleteCategory);
            NavigateToStoresCommand = new AsyncRelayCommand(NavigateToStores);
        }
        public CategoriesViewModel(ShoppingList list)
        {
            ShoppingList = list;
            AddCategoryCommand = new AsyncRelayCommand(AddCategory);
            NavigateToStoresCommand = new AsyncRelayCommand(NavigateToStores);
        }

        void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("load", out var value))
            {
                string listName = value as string;

                var loaded = ShoppingList.Load(listName);

                ShoppingList = loaded;
            }
        }

        private async Task AddCategory()
        {
            string name = await App.Current.MainPage.DisplayPromptAsync(
                "Nowa kategoria",
                "Podaj nazwę kategorii:");

            if (string.IsNullOrWhiteSpace(name))
                return;

            var newCategory = new Category { Name = name };

            ShoppingList.AddCategory(newCategory);
        }

        private async Task DeleteCategory(Category category)
        {
            if (category == null) return;
            bool ok = await App.Current.MainPage.DisplayAlert("Usuń", $"Usunąć kategorię '{category.Name}'?", "Tak", "Nie");
            if (!ok) return;
            category.DeleteCategory(_shoppingList.Name, category.Name);
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_shoppingList.Name}");
        }
        private async Task NavigateToStores()
        {
            await Shell.Current.GoToAsync("//StoresProductsPage");
        }
    }
}
