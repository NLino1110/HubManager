using CommunityToolkit.Mvvm.Input;
using DMDataSafePreview.Models;

namespace DMDataSafePreview.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}