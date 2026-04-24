# Dev 2 — Nutrition + Open Food Facts Guide

**Your branch:** `dev2/nutrition-food`
**Files you own:** `IronMind.Services/NutritionService.cs`, `IronMind.API/Program.cs`
**No schema changes needed.**

---

## What Already Exists

- `POST /nutrition/meals` — log a meal manually
- `GET /nutrition/meals/summary` — daily calorie summary
- `DELETE /nutrition/meals/{id}` — delete a meal log
- `GET /nutrition/food/search?q=banana` — route is registered but the service throws `NotImplementedException`

## What You Are Building

Implement `SearchFoodAsync` in `NutritionService` to call the Open Food Facts API and return real results.

No API key. No account. Free. Works immediately.

---

## Step 1 — Register HttpClient in Program.cs

Open `IronMind.API/Program.cs` and add this line in the service registration block, before `var app = builder.Build()`:

```csharp
builder.Services.AddHttpClient("OpenFoodFacts", client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "IronMind/1.0 (school project)");
    client.Timeout = TimeSpan.FromSeconds(5);
});
```

The `User-Agent` header is required by Open Food Facts. Without it, requests may be rejected.

---

## Step 2 — Inject HttpClient into NutritionService

Open `IronMind.Services/NutritionService.cs`.

Change the class declaration to inject an `IHttpClientFactory`:

```csharp
public class NutritionService(AppDbContext db, IHttpClientFactory httpClientFactory) : INutritionService
```

---

## Step 3 — Implement SearchFoodAsync

Add these `using` statements at the top of `NutritionService.cs` if not already present:

```csharp
using System.Net.Http.Json;
using System.Text.Json.Serialization;
```

Replace the existing `SearchFoodAsync` method (the one that throws `NotImplementedException`) with this:

```csharp
public async Task<IEnumerable<FoodSearchResult>> SearchFoodAsync(string query)
{
    var client = httpClientFactory.CreateClient("OpenFoodFacts");

    var url = $"cgi/search.pl?search_terms={Uri.EscapeDataString(query)}" +
              "&search_simple=1&action=process&json=1&page_size=10" +
              "&fields=code,product_name,nutriments";

    OffSearchResponse? response;
    try
    {
        response = await client.GetFromJsonAsync<OffSearchResponse>(url);
    }
    catch
    {
        return [];
    }

    if (response?.Products is null) return [];

    return response.Products
        .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
        .Select(p => new FoodSearchResult(
            Id: p.Code ?? string.Empty,
            Name: p.ProductName!,
            CaloriesPer100g: p.Nutriments?.EnergyKcal100g ?? 0,
            ProteinPer100g: p.Nutriments?.Proteins100g,
            CarbsPer100g: p.Nutriments?.Carbohydrates100g,
            FatPer100g: p.Nutriments?.Fat100g));
}
```

Then add these private classes at the bottom of the file, outside the `NutritionService` class but inside the `namespace`:

```csharp
file class OffSearchResponse
{
    [JsonPropertyName("products")]
    public List<OffProduct>? Products { get; set; }
}

file class OffProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("nutriments")]
    public OffNutriments? Nutriments { get; set; }
}

file class OffNutriments
{
    [JsonPropertyName("energy-kcal_100g")]
    public float EnergyKcal100g { get; set; }

    [JsonPropertyName("proteins_100g")]
    public float? Proteins100g { get; set; }

    [JsonPropertyName("carbohydrates_100g")]
    public float? Carbohydrates100g { get; set; }

    [JsonPropertyName("fat_100g")]
    public float? Fat100g { get; set; }
}
```

The `file` modifier means these classes are only visible inside this file — they're implementation details, not part of the public API.

---

## Step 4 — Build and Test

```bash
cd IronMind.API
dotnet build
dotnet run
```

Open `http://localhost:5235/swagger`.

**Test flow:**
1. `POST /auth/login` — get a token
2. Click **Authorize** in Swagger, paste `Bearer <your_token>`
3. `GET /nutrition/food/search` — enter `banana` as the `q` parameter
4. You should get back a list of products with names and calorie values

**Also test edge cases:**
- Search for something obscure — should return empty list, not crash
- If you disconnect your internet, the API should return an empty list (not 500)

---

## Done Looks Like

- `GET /nutrition/food/search?q=banana` returns real food items from Open Food Facts
- A failed or timed-out API call returns an empty list — it does not crash the endpoint
- The endpoint still requires a valid JWT (it's in the authorized group)
- `dotnet build` has zero errors or warnings related to your changes
