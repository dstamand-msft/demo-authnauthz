using System.ComponentModel.DataAnnotations;

namespace Demo.App.Options;

public class APIOptions
{

    /// <summary>
    /// Gets or sets the BaseUrl of the API
    /// </summary>
    [Required]
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the scopes required to call this API
    /// </summary>
    [Required]
    public List<string> Scopes { get; set; }
}