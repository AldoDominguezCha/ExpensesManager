using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpnesesManager.Models
{
    public class CreateAccountViewModel : Account
    {
        public IEnumerable<SelectListItem> AccountTypes { get; set; }

    }
}
