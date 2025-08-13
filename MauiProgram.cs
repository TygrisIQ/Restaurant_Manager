using Microsoft.Extensions.Logging;
using Restaurant_Manager.Data;

namespace Restaurant_Manager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            //database check
            _ = Task.Run(Db.Initialize);
            _ = Task.Run(Utility.DatabaseUtility.DbTest);
            return builder.Build();
        }
    }
}
