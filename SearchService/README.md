# SearchService

Minimal ASP.NET Core microservice with a single `/search` API that checks whether a query string exists in the product name list loaded from `data/products.json`.

## Run

```bash
dotnet run --project SearchService/SearchService.csproj
```

By default it loads products from `data/products.json` (copied to output on build). You can override the path with:

```bash
dotnet run --project SearchService/SearchService.csproj -- --Products:Path=/absolute/path/to/products.json
```

## API

- `GET /search?q=...` -> `{ "query": "...", "count": 2, "results": [ { "name": "...", "price": 0, "manufacturer": "..." } ] }`
- `POST /search` with JSON body `{ "query": "..." }` -> `{ "query": "...", "count": 2, "results": [ ... ] }`
