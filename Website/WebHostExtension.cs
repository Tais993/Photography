namespace Website;

public static class WebHostExtension
{
    extension(WebApplication app)
    {
        public void RunWeb()
        {
            app.ConfigureWeb();

            app.Run();
        }

        public void ConfigureWeb()
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
                .WithStaticAssets();
        }
    }

}