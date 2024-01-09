using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class TotalPriceCalculatedEvent
    {
        public interface ITotalPriceCalculatedEvent { }

        public record TotalPriceCalculationSucceededEvent : ITotalPriceCalculatedEvent 
        {
            public string Csv{ get;}
            public DateTime PublishedDate { get; }

            internal TotalPriceCalculationSucceededEvent(string csv, DateTime publishedDate)
            {
                Csv = csv;
                PublishedDate = publishedDate;
            }
        }

        public record TotalPriceCalculationFaildEvent : ITotalPriceCalculatedEvent 
        {
            public string Reason { get; }

            internal TotalPriceCalculationFaildEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
