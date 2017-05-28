using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Payments;
using System;
using Uol.PagSeguro;

namespace NopBrasil.Plugin.Payments.PagSeguro.Services
{
    public class PagSeguroService : IPagSeguroService
    {
        //colocar a moeda utilizadas como configuração
        private const string CURRENCY_CODE = "BRL";

        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public PagSeguroService(ISettingService settingService, ICurrencyService currencyService, CurrencySettings currencySettings)
        {
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
        }

        public Uri CreatePayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Seta as credenciais
            PagSeguroPaymentSetting pagSeguroPaymentSetting = _settingService.LoadSetting<PagSeguroPaymentSetting>();
            AccountCredentials credentials = new AccountCredentials(@pagSeguroPaymentSetting.PagSeguroEmail, @pagSeguroPaymentSetting.PagSeguroToken);

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
            payment.Sender.Name = postProcessPaymentRequest.Order.BillingAddress.FirstName + " " + postProcessPaymentRequest.Order.BillingAddress.LastName;
        }

        private decimal GetConvertedRate(decimal rate, PostProcessPaymentRequest postProcessPaymentRequest)
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
        }

        private void LoadingItems(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            foreach (var product in postProcessPaymentRequest.Order.OrderItems)
            {
                Item item = new Item();
                item.Amount = Math.Round(GetConvertedRate(product.UnitPriceInclTax, postProcessPaymentRequest), 2);
                item.Description = product.Product.Name;
                item.Id = product.Id.ToString();
                item.Quantity = product.Quantity;
                if (product.ItemWeight.HasValue)
                    item.Weight = Convert.ToInt64(product.ItemWeight);
                payment.Items.Add(item);
            }
        }
    }
}

