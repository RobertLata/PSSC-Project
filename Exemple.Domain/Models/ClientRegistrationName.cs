using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    public record ClientRegistrationName
    {
        private static readonly Regex ValidPattern = new("^[a-zA-Z]+$");

        public string Value { get; }

        internal ClientRegistrationName(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidClientRegistrationNameException("");
            }
        }

        private static bool IsValid(string stringValue) => ValidPattern.IsMatch(stringValue);

        public override string ToString()
        {
            return Value;
        }

        public static Option<ClientRegistrationName> TryParse(string stringValue)
        {
            if (IsValid(stringValue))
            {
                return Some<ClientRegistrationName>(new(stringValue));
            }
            else
            {
                return None;
            }
        }
    }
}
