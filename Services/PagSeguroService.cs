using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using Uol.PagSeguro;

namespace NopBrasil.Plugin.Payments.PagSeguro.Services
{
    public class PagSeguroService : IPagSeguroService
    {
        //todo: colocar a moeda utilizada como configuração
        private const string CURRENCY_CODE = "BRL";

        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly PagSeguroPaymentSetting _pagSeguroPaymentSetting;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;

        public PagSeguroService(ISettingService settingService, ICurrencyService currencyService, CurrencySettings currencySettings, PagSeguroPaymentSetting pagSeguroPaymentSetting, IOrderService orderService, IOrderProcessingService orderProcessingService, IStoreContext storeContext)
        {
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._pagSeguroPaymentSetting = pagSeguroPaymentSetting;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._storeContext = storeContext;
        }

        public Uri CreatePayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Seta as credenciais
            AccountCredentials credentials = new AccountCredentials(@_pagSeguroPaymentSetting.PagSeguroEmail, @_pagSeguroPaymentSetting.PagSeguroToken);

            PaymentRequest payment = new PaymentRequest();
            payment.Currency = CURRENCY_CODE;
            payment.Reference = postProcessPaymentRequest.Order.Id.ToString();

            LoadingItems(postProcessPaymentRequest, payment);
            LoadingShipping(postProcessPaymentRequest, payment);
            LoadingSender(postProcessPaymentRequest, payment);

            return Uol.PagSeguro.PaymentService.Register(credentials, payment);
        }

        private void LoadingSender(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            payment.Sender = new Sender();
            payment.Sender.Email = postProcessPaymentRequest.Order.Customer.Email;
            payment.Sender.Name = $"{postProcessPaymentRequest.Order.BillingAddress.FirstName} {postProcessPaymentRequest.Order.BillingAddress.LastName}";
        }

        private decimal GetConvertedRate(decimal rate)
        {
            var usedCurrency = _currencyService.GetCurrencyByCode(CURRENCY_CODE);
            if (usedCurrency == null)
                throw new NopException($"PagSeguro payment service. Could not load \"{CURRENCY_CODE}\" currency");

            if (usedCurrency.CurrencyCode == _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode)
                return rate;
            else
                return _currencyService.ConvertFromPrimaryStoreCurrency(rate, usedCurrency);
        }

        private void LoadingShipping(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            payment.Shipping = new Shipping();
            payment.Shipping.ShippingType = ShippingType.NotSpecified;
            Address adress = new Address();
            adress.Complement = string.Empty;
            adress.District = string.Empty;
            adress.Number = string.Empty;
            if (postProcessPaymentRequest.Order.ShippingAddress != null)
            {
                adress.City = postProcessPaymentRequest.Order.ShippingAddress.City;
                adress.Country = postProcessPaymentRequest.Order.ShippingAddress.Country.Name;
                adress.PostalCode = postProcessPaymentRequest.Order.ShippingAddress.ZipPostalCode;
                adress.State = postProcessPaymentRequest.Order.ShippingAddress.StateProvince.Name;
                adress.Street = postProcessPaymentRequest.Order.ShippingAddress.Address1;
            }
            payment.Shipping.Cost = Math.Round(GetConvertedRate(postProcessPaymentRequest.Order.OrderShippingInclTax), 2);
        }

        private void LoadingItems(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            foreach (var product in postProcessPaymentRequest.Order.OrderItems)
            {
                Item item = new Item();
                item.Amount = Math.Round(GetConvertedRate(product.UnitPriceInclTax), 2);
                item.Description = product.Product.Name;
                item.Id = product.Id.ToString();
                item.Quantity = product.Quantity;
                if (product.ItemWeight.HasValue)
                    item.Weight = Convert.ToInt64(product.ItemWeight);
                payment.Items.Add(item);
            }
        }

        private IEnumerable<Order> GetPendingOrders() => _orderService.SearchOrders(_storeContext.CurrentStore.Id, paymentMethodSystemName: "Payments.PagSeguro", psIds: new List<int>() { 10 }).Where(o => _orderProcessingService.CanMarkOrderAsPaid(o));

        private TransactionSummary GetTransaction(AccountCredentials credentials, string referenceCode) => TransactionSearchService.SearchByReference(credentials, referenceCode)?.Items?.FirstOrDefault();

        private bool TransactionIsPaid(TransactionSummary transaction) => (transaction?.TransactionStatus == TransactionStatus.Paid || transaction?.TransactionStatus == TransactionStatus.Available);

        public void CheckPayments()
        {
            AccountCredentials credentials = new AccountCredentials(@_pagSeguroPaymentSetting.PagSeguroEmail, @_pagSeguroPaymentSetting.PagSeguroToken);
            foreach (var order in GetPendingOrders())
                if (TransactionIsPaid(GetTransaction(credentials, order.Id.ToString())))
                    _orderProcessingService.MarkOrderAsPaid(order);
        }
    }
}