﻿using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using OShop.Models;
using System;
using System.Linq;

namespace OShop.Services.ShoppingCartResolvers {
    [OrchardFeature("OShop.Checkout")]
    public class CustomerResolver : IShoppingCartBuilder, IOrderBuilder {
        private readonly ICustomersService _customersService;
        private readonly ILocationsService _locationsService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CustomerResolver (
            ICustomersService customersService,
            ILocationsService locationsService,
            IWorkContextAccessor workContextAccessor) {
            _customersService = customersService;
            _locationsService = locationsService;
            _workContextAccessor = workContextAccessor;
        }

        public int Priority {
            get { return 900; }
        }

        public void BuildCart(IShoppingCartService ShoppingCartService, ShoppingCart Cart) {
            var customer = _customersService.GetCustomer();

            if (customer == null) {
                return;
            }

            // Get "Checkout" property to know the Checkout provider beeing used
            var checkout = ShoppingCartService.GetProperty<string>("Checkout");

            if (string.IsNullOrWhiteSpace(checkout) && customer.DefaultAddress != null) {
                // Override default location
                ShoppingCartService.SetProperty<int>("CountryId", customer.DefaultAddress.CountryId);
                ShoppingCartService.SetProperty<int>("StateId", customer.DefaultAddress.StateId);

                Cart.Properties["BillingCountry"] = customer.DefaultAddress.Country;
                Cart.Properties["BillingState"] = customer.DefaultAddress.State;
                Cart.Properties["ShippingCountry"] = customer.DefaultAddress.Country;
                Cart.Properties["ShippingState"] = customer.DefaultAddress.State;
            }
            else if (checkout == "Checkout") {
                var billingAddress = customer.Addresses.Where(a => a.Id == ShoppingCartService.GetProperty<int>("BillingAddressId")).FirstOrDefault() ?? customer.DefaultAddress ?? customer.Addresses.FirstOrDefault();
                var shippingAddress = customer.Addresses.Where(a => a.Id == ShoppingCartService.GetProperty<int>("ShippingAddressId")).FirstOrDefault() ?? customer.DefaultAddress ?? customer.Addresses.FirstOrDefault();

                if (billingAddress != null) {
                    Cart.Properties["BillingAddress"] = billingAddress;
                    Cart.Properties["BillingCountry"] = billingAddress.Country;
                    Cart.Properties["BillingState"] = billingAddress.State;
                }

                if (shippingAddress != null) {
                    Cart.Properties["ShippingAddress"] = shippingAddress;
                    Cart.Properties["ShippingCountry"] = shippingAddress.Country;
                    Cart.Properties["ShippingState"] = shippingAddress.State;
                }
            }
        }

        public void BuildOrder(IShoppingCartService ShoppingCartService, IContent Order) {
            var customer = _customersService.GetCustomer();

            if (customer == null) {
                return;
            }

            var customerOrderPart = Order.As<CustomerOrderPart>();
            if (customerOrderPart != null) {
                customerOrderPart.Customer = customer;

                Int32 billingAddressId = ShoppingCartService.GetProperty<int>("BillingAddressId");
                if (billingAddressId > 0) {
                    var billingAddress = customer.Addresses.Where(a => a.Id == billingAddressId).FirstOrDefault();
                    if (billingAddress != null) {
                        customerOrderPart.BillingAddress = billingAddress;
                    }
                }
            }

            var shippingPart = Order.As<OrderShippingPart>();
            if (shippingPart != null) {
                //  Shipping address
                Int32 shippingAddressId = ShoppingCartService.GetProperty<int>("ShippingAddressId");
                if (shippingAddressId > 0) {
                    var shippingAddress = customer.Addresses.Where(a => a.Id == shippingAddressId).FirstOrDefault();
                    if (shippingAddress != null) {
                        // Set address
                        if (customerOrderPart != null) {
                            customerOrderPart.ShippingAddress = shippingAddress;
                        }

                        // Set shipping zone
                        var workContext = _workContextAccessor.GetContext();
                        var state = _locationsService.GetState(shippingAddress.StateId);
                        var country = _locationsService.GetCountry(shippingAddress.CountryId);
                        if (state != null && state.Enabled && state.ShippingZoneRecord != null) {
                            workContext.SetState("OShop.Orders.ShippingZone", state.ShippingZoneRecord);
                        }
                        else if (country != null && country.Enabled && country.ShippingZoneRecord != null) {
                            workContext.SetState("OShop.Orders.ShippingZone", country.ShippingZoneRecord);
                        }
                    }
                }
            }
        }

    }
}