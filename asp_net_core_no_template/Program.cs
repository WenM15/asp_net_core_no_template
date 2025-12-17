namespace asp_net_core_no_template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Simple route
            app.MapGet("/", () => "Replying to GET /");
            app.Run();
        }
    }
}
