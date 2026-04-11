using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

var productsPath = builder.Configuration["Products:Path"] ?? "data/products.json";
var resolvedProductsPath = Path.IsPathRooted(productsPath)
    ? productsPath
    : Path.Combine(AppContext.BaseDirectory, productsPath);

var productSearchIndex = ProductSearchIndex.LoadFromFile(resolvedProductsPath);
builder.Services.AddSingleton(productSearchIndex);

var app = builder.Build();

app.MapGet("/", () => Results.Text("SearchService is running."));

app.MapGet("/search", (string? q, ProductSearchIndex index) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { error = "Missing query. Provide ?q=..." });
    }

    var matches = index.Search(q);
    return Results.Ok(new { query = q, count = matches.Count, results = matches });
});

app.MapPost("/search", (SearchRequest request, ProductSearchIndex index) =>
{
    if (string.IsNullOrWhiteSpace(request.Query))
    {
        return Results.BadRequest(new { error = "Missing query. Provide JSON body { \"query\": \"...\" }" });
    }

    var matches = index.Search(request.Query);
    return Results.Ok(new { query = request.Query, count = matches.Count, results = matches });
});

app.Run();

internal sealed record SearchRequest(string Query);
