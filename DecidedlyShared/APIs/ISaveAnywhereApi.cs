using System;

namespace DecidedlyShared.APIs;

public interface ISaveAnywhereApi
{
    event EventHandler BeforeSave;
    event EventHandler AfterLoad;
}
