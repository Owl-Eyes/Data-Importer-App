using System;

public class FundsData
{
    public string FundName { get; set; }
    public string JSECode { get; set; }
    public string ISIN { get; set; }
    public decimal? OneYearPerf { get; set; }
    public decimal? ThreeYearsPerf { get; set; }
    public decimal? FiveYearsPerf { get; set; }
    public decimal? TenYearsPerf { get; set; }
    public decimal? YTDPerf { get; set; }
    public string AsAtDate { get; set; }
    public string RiskRating { get; set; }
}

// By J. Koekemoer