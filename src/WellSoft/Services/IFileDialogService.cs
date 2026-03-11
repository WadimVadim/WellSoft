using WellSoft.Models;

namespace WellSoft.Services;

public interface IFileDialogService
{
    string OpenFileDialog(string filter);
    string SaveFileDialog(string filter, string defaultFileName = null);
}
