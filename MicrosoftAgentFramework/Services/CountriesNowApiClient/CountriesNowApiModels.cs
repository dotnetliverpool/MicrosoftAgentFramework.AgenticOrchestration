namespace MicrosoftAgentFramework.Services.CountriesNowApiClient.Models;

// Request Models
public class CityPopulationRequest
{
    public string City { get; set; } = string.Empty;
}

public class FilterCitiesRequest
{
    public int? Limit { get; set; }
    public string? Order { get; set; }
    public string? OrderBy { get; set; }
    public string? Country { get; set; }
}

public class FilterPopulationRequest
{
    public int? Year { get; set; }
    public int? Limit { get; set; }
    public long? Lt { get; set; }
    public long? Gt { get; set; }
    public string? OrderBy { get; set; }
    public string? Order { get; set; }
}

public class CountryPopulationRequest
{
    public string Country { get; set; } = string.Empty;
}

public class CountryCurrencyRequest
{
    public string Country { get; set; } = string.Empty;
}

public class CountryPositionRequest
{
    public string Country { get; set; } = string.Empty;
}

// Response Models
public class ApiResponse<T>
{
    public bool Error { get; set; }
    public string Msg { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class CityPopulationData
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<PopulationCount> PopulationCounts { get; set; } = new();
}

public class CountryPopulationData
{
    public string Country { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Iso3 { get; set; }
    public List<YearValuePopulation> PopulationCounts { get; set; } = new();
}

public class FilteredCountryPopulationData
{
    public string Country { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public YearValuePopulation PopulationCounts { get; set; } = new();
}

public class CountryCurrencyData
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Iso2 { get; set; } = string.Empty;
    public string Iso3 { get; set; } = string.Empty;
}

public class CountryPositionData
{
    public string Name { get; set; } = string.Empty;
    public string Iso2 { get; set; } = string.Empty;
    public double Long { get; set; }
    public double Lat { get; set; }
}

public class PopulationCount
{
    public string Year { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string Reliabilty { get; set; } = string.Empty;
}

public class YearValuePopulation
{
    public int Year { get; set; }
    public long Value { get; set; }
}
