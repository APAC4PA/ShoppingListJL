using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using ShoppingListJL.Models;

namespace ShoppingListJL.ViewModels
{
    internal class StoresProductsViewModel : ObservableObject
    {
        public ObservableCollection<string> Stores { get; } = new ObservableCollection<string>();
        public ObservableCollection<ProductItem> FilteredProducts { get; } = new ObservableCollection<ProductItem>();

        private string _selectedStore = string.Empty;
        public string SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (SetProperty(ref _selectedStore, value))
                    UpdateFilteredProducts();
            }
        }

        public ICommand RefreshCommand { get; }

        private List<ProductItem> _allProducts = new();

        public StoresProductsViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadStoresAndProducts);

            _ = LoadStoresAndProducts();
        }

        private async Task LoadStoresAndProducts()
        {
            Stores.Clear();
            _allProducts.Clear();
            FilteredProducts.Clear();

            try
            {
                var allLists = ShoppingList.LoadAll();
                foreach (var sl in allLists)
                {
                    foreach (var cat in sl.Categories)
                    {
                        foreach (var p in cat.Products)
                        {
                            var store = string.IsNullOrWhiteSpace(p.Store) ? "(Brak sklepu)" : p.Store;
                            var item = new ProductItem
                            {
                                Name = p.Name,
                                Quantity = p.Quantity,
                                Unit = p.Unit,
                                Store = store,
                                ListName = sl.Name,
                                CategoryName = cat.Name
                            };
                            _allProducts.Add(item);
                        }
                    }
                }

                var stores = _allProducts.Select(x => x.Store).Distinct().OrderBy(s => s).ToList();
                Stores.Add("Wszystkie sklepy");
                foreach (var s in stores)
                    Stores.Add(s);

                SelectedStore = "Wszystkie sklepy";
            }
            catch
            {
                Trace.WriteLine("B³¹d podczas ³adowania list zakupów.");
            }

            await Task.CompletedTask;
        }

        private void UpdateFilteredProducts()
        {
            FilteredProducts.Clear();
            if (string.IsNullOrEmpty(SelectedStore) || SelectedStore == "Wszystkie sklepy")
            {
                foreach (var it in _allProducts)
                    FilteredProducts.Add(it);
            }
            else
            {
                foreach (var it in _allProducts.Where(p => p.Store == SelectedStore))
                    FilteredProducts.Add(it);
            }
        }

        internal class ProductItem
        {
            public string Name { get; init; } = string.Empty;
            public string Quantity { get; init; } = string.Empty;
            public string Unit { get; init; } = string.Empty;
            public string Store { get; init; } = string.Empty;
            public string ListName { get; init; } = string.Empty;
            public string CategoryName { get; init; } = string.Empty;
        }
    }
}