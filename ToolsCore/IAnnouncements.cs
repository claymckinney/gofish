using System.Collections.ObjectModel;

namespace ToolsCore
{
    public interface IAnnouncements
    {
        ReadOnlyCollection<string> AnnouncementsList { get; }

        void Add(string announcement);
        void ReadAll();
    }
}