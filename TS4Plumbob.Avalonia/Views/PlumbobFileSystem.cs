using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace TS4Plumbob.Avalonia.Views;

public static class PlumbobFileSystem
{

    /// <summary>
    /// Platform-specific implementation of the folder picking logic.
    /// This uses the Avalonia StorageProvider API.
    /// </summary>
    /// <returns>The selected IStorageFolder, or null if cancelled.</returns>
    public static async Task<IStorageFolder?> PickFolder(Visual displayer, string? pickerTitle)
    {
        var topLevel = TopLevel.GetTopLevel(displayer);
        if (topLevel == null) return null;

        var sp = topLevel.StorageProvider;

        // Try to start the picker in the user's Documents folder.
        Task<IStorageFolder?> getStartLocTask =
            sp.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        IReadOnlyList<IStorageFolder> folders = await sp.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                Title = pickerTitle,
                SuggestedStartLocation = await getStartLocTask,
                AllowMultiple = false
            });

        return folders.Count >= 1 ? folders[0] : null;
    }
}