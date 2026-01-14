namespace asp_net_core_no_template.Endpoints
{
    public static class ParameterBindingEndpoints
    {
        public static void Register(WebApplication app)
        {
            var services = new[]
            {
                new { Name = "MockApi", State = "OK", Dependencies = new[] { "MockCache" } },
                new { Name = "MockCache", State = "Degraded", Dependencies = new[] { "MockDb" } },
                new { Name = "MockDb", State = "OK", Dependencies = Array.Empty<string>() }
            };

            // Simple guideline on when to use route values and when to use query:
            // Route identifies what resource. Query modifies how much data is returned.

            // Route binding.
            // Try:
            // /status/MockApi
            // /status/MockCache
            // /status/MockDb
            // /status/RealApi
            // /status/MockApi?service=other (service bounded to MockApi instead to other, because route values have higher binding precedence)
            app.MapGet("status/{service}", (string service) =>
            {
                var match = services.FirstOrDefault(s =>
                string.Equals(s.Name, service, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    return Results.NotFound(new { Service = service, Error = "Service not found" });
                }

                return Results.Ok(new { Service = match.Name, State = match.State });
            });

            // Query binding.
            // Try:
            // /services
            // /services?state=Ok
            // /services?state=Degraded
            app.MapGet("/services", (string? state) =>
            {
                var result = state == null ? services : services.Where(s => string.Equals(s.State, state, StringComparison.OrdinalIgnoreCase));

                return Results.Ok(result.Select(s => new { s.Name, s.State }));
            });

            // Route + Query binding.
            // Try:
            // /dependencies/MockApi
            // /dependencies/MockApi?recursive=true
            app.MapGet("dependencies/{service}", (string service, bool recursive = false) =>
            {
                var match = services.FirstOrDefault(s =>
                string.Equals(s.Name, service, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    return Results.NotFound(new
                    {
                        Service = service,
                        Error = "Service not found"
                    });
                }

                var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                void Collect(string name)
                {
                    var s = services.First(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                    foreach (var d in s.Dependencies)
                    {
                        if (result.Add(d) && recursive)
                        {
                            Collect(d);
                        }
                    }
                }

                Collect(match.Name);

                return Results.Ok(new
                {
                    Service = match.Name,
                    Dependencies = result.ToArray()
                });
            });
        }
    }
}
