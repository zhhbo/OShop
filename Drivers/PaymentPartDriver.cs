﻿using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using OShop.Models;
using OShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OShop.Drivers {
    [OrchardFeature("OShop.Payment")]
    public class PaymentPartDriver : ContentPartDriver<PaymentPart> {
        private readonly ICurrencyProvider _currencyProvider;

        private const string TemplateName = "Parts/Payment";

        public PaymentPartDriver(ICurrencyProvider currencyProvider) {
            _currencyProvider = currencyProvider;
        }

        protected override string Prefix { get { return "Payment"; } }

        protected override DriverResult Display(PaymentPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Payment", () => shapeHelper.Parts_Payment(
                ContentPartDriver: part,
                NumberFormat: _currencyProvider.NumberFormat
            ));
        }


    }
}