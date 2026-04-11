namespace SearchService.Models;

public sealed record Product(
    string Name,
    decimal Price,
    string Manufacturer
);

