using System.Text.Json.Serialization;

namespace Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

public class Supplier
{
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }
    [JsonPropertyName("supplierName")]
    public string SupplierName { get; set; } = string.Empty;
}