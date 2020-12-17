using System;
using System.Collections.Generic;
using System.Text;

namespace IChiba.Services.Master
{
    public class VatTypeSearchContext
    {
        public string Keywords { get; set; }

        public int Status { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string LanguageId { get; set; }
    }
}
