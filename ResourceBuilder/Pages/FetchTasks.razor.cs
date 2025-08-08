
using Models.DMSA.Shared.Tools;

namespace ResourceBuilder.Pages
{
    public partial class FetchTasks
    {
        protected override async Task OnInitializedAsync()
        {
            await LoadTasks();
        }

        private async Task LoadTasks()
        {
            //CronSettings.Setup();
            //var tasks = await CronSettings.GetTask();

            foreach (var taskItem in ConfigurationHelper.GetAppSettings().profile.Tasks)
            {
                Console.WriteLine(taskItem.schedule);
            }
        }
    }
}
