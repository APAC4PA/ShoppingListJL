using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingListJL.Models;
using System.Diagnostics;

namespace ShoppingListJL.ViewModels
{
    internal class ProductsViewModel : ObservableObject
    {
        private Category _category;
        private ShoppingList _list;

        public Category Category { get => _category; set => SetProperty(ref _category, value); }
        public ShoppingList List { get => _list; set => SetProperty(ref _list, value); }
        public ICommand CheckItemCommand { get; }
        public ICommand ShowNewItemForm { get; }
        public ICommand AddNewItem { get; }
        public ICommand Close { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand IncrementQuantityCommand { get; }
        public ICommand DecrementQuantityCommand { get; }
        public ICommand ShowItemsCommand { get; }
        public ICommand OptionalItemCommand { get; }

        private bool _isShown = true;
        public bool IsShown
        { get => _isShown; set => SetProperty(ref _isShown, value); }
        private bool _isAdding;
        public bool IsAdding { get => _isAdding; set => SetProperty(ref _isAdding, value); }

        private string _newName = string.Empty;
        public string NewName { get => _newName; set => SetProperty(ref _newName, value); }

        private string _newQuantity = string.Empty;
        public string NewQuantity { get => _newQuantity; set => SetProperty(ref _newQuantity, value); }

        private string _newUnit = string.Empty;
        public string NewUnit { get => _newUnit; set => SetProperty(ref _newUnit, value); }
        private bool _isOptional = false;
        public bool IsOptional { get => _isOptional; set => SetProperty(ref _isOptional, value); }
        private string _newStore = string.Empty;
        public string NewStore { get => _newStore; set => SetProperty(ref _newStore, value); }

        public ProductsViewModel()
        {
            _category = new Category();
            _list = new ShoppingList();
            DeleteItemCommand = new AsyncRelayCommand<Product>(DeleteItem);
            CheckItemCommand = new AsyncRelayCommand<Product>(CheckItem);
            ShowItemsCommand = new AsyncRelayCommand(ShowItems);
            OptionalItemCommand = new AsyncRelayCommand<Product>(OptionalItem);
            IncrementQuantityCommand = new AsyncRelayCommand<Product>(IncrementQuantity);
            DecrementQuantityCommand = new AsyncRelayCommand<Product>(DecrementQuantity);
            ShowNewItemForm = new AsyncRelayCommand(() => { IsAdding = true; return Task.CompletedTask; });
            AddNewItem = new AsyncRelayCommand(AddItem);
            Close = new AsyncRelayCommand(() => { IsAdding = false; return Task.CompletedTask; });
        }
        public ProductsViewModel(Category category, ShoppingList list)
        {
            _category = category;
            _list = list;
            DeleteItemCommand = new AsyncRelayCommand<Product>(DeleteItem);
            CheckItemCommand = new AsyncRelayCommand<Product>(CheckItem);
            ShowItemsCommand = new AsyncRelayCommand(ShowItems);
            OptionalItemCommand = new AsyncRelayCommand<Product>(OptionalItem);
            IncrementQuantityCommand = new AsyncRelayCommand<Product>(IncrementQuantity);
            DecrementQuantityCommand = new AsyncRelayCommand<Product>(DecrementQuantity);
            ShowNewItemForm = new AsyncRelayCommand(() => { IsAdding = true; return Task.CompletedTask; });
            AddNewItem = new AsyncRelayCommand(AddItem);
            Close = new AsyncRelayCommand(() => { IsAdding = false; return Task.CompletedTask; });
        }
        private async Task OptionalItem(Product p)
        {
            if (p == null) return;
            Trace.WriteLine("Toggling optional for item: " + p.Name + " to " + p.Optional);
            _list?.Save();
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_list?.Name}");
        }
        private async Task ShowItems()
        {
            Trace.WriteLine("Toggling item visibility"+ IsShown);
            IsShown = !IsShown;
        }
        private async Task IncrementQuantity(Product product)
        {
            if (product == null) return;
            if (int.TryParse(product.Quantity, out int q) || product.Quantity == "Not specified")
                q++;
            else
                q = 1;
            product.Quantity = q.ToString();
            _list?.Save();
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_list.Name}");
            await Task.CompletedTask;
        }

        private async Task DecrementQuantity(Product product)
        {
            if (product == null) return;

            if (int.TryParse(product.Quantity, out int q) && q > 0 || product.Quantity == "Not specified")
            {
                q--;
                product.Quantity = q.ToString();
                _list?.Save();
            }
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_list.Name}");
            await Task.CompletedTask;
        }
        private async Task AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewName))
                NewName = "Unnamed Product";
            if (string.IsNullOrWhiteSpace(NewQuantity))
                NewQuantity = "Not specified";
            if (string.IsNullOrWhiteSpace(NewUnit))
                NewUnit = "Not specified";
            var product = new Product
            {
                Name = NewName,
                Quantity = NewQuantity,
                Unit = NewUnit,
                Optional = IsOptional.ToString(),
                Store = NewStore ?? string.Empty
            };

            _category.AddProduct(product);

            NewName = string.Empty;
            NewQuantity = string.Empty;
            NewUnit = string.Empty;
            NewStore = string.Empty;
            IsAdding = false;
        }
        private async Task CheckItem(Product product)
        {
            product.MarkProductAsBought(_list.Name, _category.Name, product.Name);
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_list.Name}");
        }
        private async Task DeleteItem(Product product)
        {
            Trace.WriteLine($"Deleting product: {product.Name}");
            bool ok = await App.Current.MainPage.DisplayAlert("Usuń", $"Usunąć kategorię '{product.Name}'?", "Tak", "Nie");
            if (!ok) return;
            product.DeleteProduct(_list.Name, _category.Name, product.Name);
            await Shell.Current.GoToAsync($"../{nameof(Views.CategoryPage)}?load={_list.Name}");
        }
    }
}