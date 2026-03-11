using Microsoft.Win32;
using WellSoft.Models;

namespace WellSoft.Services;

public class FileDialogService : IFileDialogService
{
    public string OpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string SaveFileDialog(string filter, string defaultFileName = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName ?? "well_data.json",
            DefaultExt = "json",
            AddExtension = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}