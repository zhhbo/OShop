﻿using Orchard;
using OShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OShop.Services {
    public interface IShoppingCartService : IDependency {
        IEnumerable<ShoppingCartItem> ListItems();
        void Add(int ItemId, string ItemType = "Product", int Quantity = 1);
        void UpdateQuantity(int Id, int Quantity);
        void Remove(int Id);
        void Empty();
    }
}