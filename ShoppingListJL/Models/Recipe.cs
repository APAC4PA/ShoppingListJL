using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ShoppingListJL.Models
{
    internal class Recipe : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Instruction{ get; set; } = string.Empty;
        public List<Product> Ingredients { get; } = new List<Product>();

        private string _targetList = string.Empty;
        public string TargetList
        {
            get => _targetList;
            set => SetProperty(ref _targetList, value);
        }

        public Recipe() { }

        public Recipe(string name, string instruction, IEnumerable<Product> ingredients)
        {
            Name = name;
            Instruction = instruction;
            Ingredients.AddRange(ingredients);
        }
    }
}