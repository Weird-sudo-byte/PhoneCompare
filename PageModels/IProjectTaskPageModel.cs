using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;

namespace PhoneCompare.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}