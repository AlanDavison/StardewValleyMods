namespace DecidedlyShared.APIs;

public interface ISavingEventArgs { }
public interface ISavedEventArgs { }
public interface ILoadingEventArgs { }
public interface ILoadedEventArgs { }

public interface IQuickSaveApi
{
    /* Save Event Order:
             * 1. QS-Saving (IsSaving = true)
             * 2. QS-Saved (IsSaving = false)
             */

    /// <summary>Fires before a Quicksave is being created</summary>
    public event SavingDelegate SavingEvent;
    /// <summary>Fires after a Quicksave has been created</summary>
    public event SavedDelegate SavedEvent;
    public bool IsSaving { get; }

    /* Load Event Order:
     * 1. QS-Loading (IsLoading = true)
     * 2. SMAPI-LoadStageChanged
     * 3. SMAPI-SaveLoaded & SMAPI-DayStarted
     * 4. QS-Loaded (IsLoading = false)
     */

    /// <summary>Fires before a Quicksave is being loaded</summary>
    public event LoadingDelegate LoadingEvent;
    /// <summary>Fires after a Quicksave was loaded</summary>
    public event LoadedDelegate LoadedEvent;
    public bool IsLoading { get; }

    public delegate void SavingDelegate(object sender, ISavingEventArgs e);
    public delegate void SavedDelegate(object sender, ISavedEventArgs e);
    public delegate void LoadingDelegate(object sender, ILoadingEventArgs e);
    public delegate void LoadedDelegate(object sender, ILoadedEventArgs e);
}
