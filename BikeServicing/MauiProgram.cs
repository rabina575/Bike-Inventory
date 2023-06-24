using BikeServicing.Data;
using MudBlazor.Services;
using Syncfusion.Blazor;

namespace BikeServicing;

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
        
#endif
        builder.Services.AddSingleton<User>();
        builder.Services.AddMudServices();
		builder.Services.AddSyncfusionBlazor();

        return builder.Build();
	}
}
