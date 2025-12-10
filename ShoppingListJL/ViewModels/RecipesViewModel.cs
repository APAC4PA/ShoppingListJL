using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using ShoppingListJL.Models;

namespace ShoppingListJL.ViewModels
{
    internal class RecipesViewModel : ObservableObject, IQueryAttributable
    {
        private ShoppingList _shoppingList;
        private string _loadedListName;

        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        public ObservableCollection<string> Lists { get; } = new ObservableCollection<string>();

        private string _selectedList = string.Empty;
        public string SelectedList
        {
            get => _selectedList;
            set => SetProperty(ref _selectedList, value);
        }

        private string _newRecipeName = string.Empty;
        public string NewRecipeName
        {
            get => _newRecipeName;
            set => SetProperty(ref _newRecipeName, value);
        }
        private string _NewRecipeInstruction = string.Empty;
        public string NewRecipeInstruction
        {
            get => _NewRecipeInstruction;
            set => SetProperty(ref _NewRecipeInstruction, value);
        }

        private string _newIngredientName = string.Empty;
        public string NewIngredientName
        {
            get => _newIngredientName;
            set => SetProperty(ref _newIngredientName, value);
        }

        private string _newIngredientQuantity = string.Empty;
        public string NewIngredientQuantity
        {
            get => _newIngredientQuantity;
            set => SetProperty(ref _newIngredientQuantity, value);
        }

        private string _newIngredientUnit = string.Empty;
        public string NewIngredientUnit
        {
            get => _newIngredientUnit;
            set => SetProperty(ref _newIngredientUnit, value);
        }

        private string _newIngredientStore = string.Empty;
        public string NewIngredientStore
        {
            get => _newIngredientStore;
            set => SetProperty(ref _newIngredientStore, value);
        }

        public ObservableCollection<Product> NewIngredients { get; } = new ObservableCollection<Product>();

        public ICommand ImportRecipeCommand { get; }
        public ICommand AddIngredientCommand { get; }
        public ICommand RemoveIngredientCommand { get; }
        public ICommand CreateRecipeCommand { get; }

        public RecipesViewModel()
        {
            ImportRecipeCommand = new AsyncRelayCommand<Recipe>(ImportRecipe);
            AddIngredientCommand = new RelayCommand(AddIngredient);
            RemoveIngredientCommand = new RelayCommand<Product>(RemoveIngredient);
            CreateRecipeCommand = new RelayCommand(CreateRecipe);

            try
            {
                var all = ShoppingList.LoadAll();
                foreach (var sl in all)
                    Lists.Add(sl.Name);
            }
            catch
            {
                Trace.WriteLine("B³¹d ³adowania list zakupów.");
            }

            Recipes.Add(new Recipe("Naleœniki", "Opis przepisu na naleœniki", new[]
            {
                new Product { Name = "M¹ka", Quantity = "200", Unit = "g", Optional = false, Store = "Biedronka" },
                new Product { Name = "Mleko", Quantity = "300", Unit = "ml", Optional = false, Store = "Biedronka" },
                new Product { Name = "Jajko", Quantity = "2", Unit = "szt", Optional = false, Store = "Biedronka" }
            }));
            Recipes.Add(new Recipe("Sa³atka grecka", "Opis przepisu na sa³atkê greck¹", new[]
            {
                new Product { Name = "Pomidor", Quantity = "2", Unit = "szt", Optional = false, Store = "Lidl" },
                new Product { Name = "Ogórek", Quantity = "1", Unit = "szt", Optional = false, Store = "Lidl" },
                new Product { Name = "Feta", Quantity = "150", Unit = "g", Optional = true, Store = "Lidl" }
            }));
        }

        void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("list", out var value))
            {
                _loadedListName = value as string ?? string.Empty;
                if (!string.IsNullOrEmpty(_loadedListName))
                {
                    _shoppingList = ShoppingList.Load(_loadedListName);
                }
            }
        }

        private void AddIngredient()
        {
            if (string.IsNullOrWhiteSpace(NewIngredientName)) return;

            var p = new Product
            {
                Name = NewIngredientName,
                Quantity = NewIngredientQuantity ?? string.Empty,
                Unit = NewIngredientUnit ?? string.Empty,
                Optional = false,
                Store = NewIngredientStore ?? string.Empty
            };
            NewIngredients.Add(p);

            NewIngredientName = string.Empty;
            NewIngredientQuantity = string.Empty;
            NewIngredientUnit = string.Empty;
            NewIngredientStore = string.Empty;
        }

        private void RemoveIngredient(Product product)
        {
            if (product == null) return;
            NewIngredients.Remove(product);
        }

        private void CreateRecipe()
        {
            if (string.IsNullOrWhiteSpace(NewRecipeName) || NewIngredients.Count == 0) return;

            var recipe = new Recipe(NewRecipeName, NewRecipeInstruction, NewIngredients.ToList());
            Recipes.Add(recipe);
            NewRecipeName = string.Empty;
            NewRecipeInstruction = string.Empty;
            NewIngredients.Clear();
        }

        private async Task ImportRecipe(Recipe recipe)
        {
            if (recipe == null) return;

            ShoppingList targetList = null;

            if (!string.IsNullOrWhiteSpace(recipe.TargetList))
            {
                try
                {
                    targetList = ShoppingList.Load(recipe.TargetList);
                }
                catch
                {
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(_loadedListName))
            {
                targetList = ShoppingList.Load(_loadedListName);
            }
            else if (_shoppingList != null)
            {
                targetList = _shoppingList;
            }
            else if (!string.IsNullOrEmpty(SelectedList))
            {
                try
                {
                    targetList = ShoppingList.Load(SelectedList);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                return;
            }

            var categoryName = string.IsNullOrWhiteSpace(recipe.Name) ? "Imported" : recipe.Name;
            await ImportRecipeIntoCategory((recipe, categoryName, targetList));
        }

        private async Task ImportRecipeIntoCategory((Recipe recipe, string category, ShoppingList targetList) args)
        {
            var recipe = args.recipe;
            var categoryName = string.IsNullOrWhiteSpace(args.category) ? "Imported" : args.category;
            var list = args.targetList;

            if (recipe == null || list == null) return;

            var targetCat = list.Categories.FirstOrDefault(c => c.Name == categoryName);
            if (targetCat == null)
            {
                targetCat = new Category { Name = categoryName, ParentList = list };
                list.AddCategory(targetCat);
            }

            foreach (var ing in recipe.Ingredients)
            {
                var p = new Product
                {
                    Name = ing.Name,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit,
                    Optional = ing.Optional,
                    Store = ing.Store
                };
                list.AddProduct(targetCat.Name, p);
            }

            list.Save();

            await Shell.Current.GoToAsync($"../{nameof(ShoppingListJL.Views.CategoryPage)}?load={Uri.EscapeDataString(list.Name)}");
        }
    }
}