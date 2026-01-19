//////using InputKit.Handlers;
//////using Plainer.Maui;

//////using UraniumUI.Dialogs;
//////using BeebTech.Controls.Handlers;
////////using BeebTech.Controls.Options;
//////using BeebTech.Controls.Views;

//////namespace BeebTech.Controls.UI;
//////public static class MauiProgramExtensions
//////{
//////    public static MauiAppBuilder UseUraniumUI(this MauiAppBuilder builder)
//////    {
//////        builder.ConfigureMauiHandlers(handlers =>
//////        {
//////            handlers.AddUraniumUIHandlers();
//////        });

//////        builder.Services.Configure<DialogOptions>(options =>
//////        {
//////            // Keep this here
//////        });

//////        builder.Services.AddTransient<IDialogService, DefaultDialogService>();

//////        //builder.ConfigureAutoFormViewDefaults();

//////        return builder;
//////    }

//////    public static IMauiHandlersCollection AddUraniumUIHandlers(this IMauiHandlersCollection collection)
//////    {
//////        return collection
//////            .AddInputKitHandlers()
//////            .AddHandler(typeof(Button), typeof(StatefulButtonHandler))
//////            .AddHandler(typeof(StatefulContentView), typeof(StatefulContentViewHandler))
//////            .AddHandler(typeof(AutoCompleteView), typeof(AutoCompleteViewHandler))
//////            .AddHandler(typeof(SelectableLabel), typeof(SelectableLabelHandler))
//////            .AddHandler(typeof(Dropdown), typeof(DropdownHandler))
//////            .AddPlainer();
//////    }
//////}
