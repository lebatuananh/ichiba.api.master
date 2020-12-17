namespace IChiba.Services.Master
{
    public class DistrictSearchContext
    {
        public string Keywords { get; set; }

        public int Status { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string LanguageId { get; set; }

        public string CountryId { get; set; }

        public string StateProvinceId { get; set; }
    }
}
