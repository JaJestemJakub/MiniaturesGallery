using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniaturesGallery.HelpClasses
{
    public enum OrderBy
    {
        [Display(Name = "Date Descending")]
        DateDesc,
        [Display(Name = "Date Ascending")]
        DateAsc,
        [Display(Name = "Rates Descending")]
        RatesDesc,
        [Display(Name = "Rates Ascending")]
        RatesAsc
    }
}
