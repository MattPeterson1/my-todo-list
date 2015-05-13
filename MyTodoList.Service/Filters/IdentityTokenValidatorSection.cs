using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;


namespace MyTodoList.Service.Filters
{
    public class IdentityTokenValidatorSection : ConfigurationSection
    {
        [ConfigurationProperty("issuer", IsRequired = true)]
        public string Issuer
        {
            get { return (string)this["issuer"]; }
        }

        [ConfigurationProperty("clientId", IsRequired = true)]
        public string ClientId
        {
            get { return (string)this["clientId"]; }
        }

        [ConfigurationProperty("clientSecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return (string)this["clientSecret"]; }
        }

        [ConfigurationProperty("redirectUri", IsRequired = true)]
        public string RedirectUri
        {
            get { return (string)this["redirectUri"]; }
        }

        [ConfigurationProperty("issuerSigningCertificate", IsRequired = true), TypeConverter(typeof(X509Certificate2TypeConverter))]
        public X509Certificate2 IssuerSigningCertificate
        {
            get
            {
                return (X509Certificate2)this["issuerSigningCertificate"];
            }
        }

        [ConfigurationProperty("tokenEndpoint", IsRequired = true)]
        public string TokenEndpoint
        {
            get { return (string)this["tokenEndpoint"]; }
        }

        [ConfigurationProperty("authorizationEndpoint", IsRequired = true)]
        public string AuthorizationEndpoint
        {
            get { return (string)this["authorizationEndpoint"]; }
        }

        [ConfigurationProperty("userinfoEndpoint", IsRequired = true)]
        public string UserinfoEndpoint
        {
            get { return (string)this["userinfoEndpoint"]; }
        }
    }

    public class X509Certificate2TypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var val = value as string;
            string[] stringSeparators = { "-----" };
            if (val != null)
            {
                var split = val.Split(stringSeparators, StringSplitOptions.None);
                var base64Cert = split.Length == 1 ? split[0] : split[1];
                return new X509Certificate2(Convert.FromBase64String(base64Cert.Trim()));
            }

            return null;
        }
    }
}
