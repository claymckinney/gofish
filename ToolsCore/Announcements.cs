using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ToolsCore
{
    public class Announcements : IAnnouncements // Just a simple log. Collects messages for later output. I don't want to think about logging right now.
    {
        private readonly List<string> _announcements;
        private readonly ILogger _logger;
        public ReadOnlyCollection<string> AnnouncementsList { get; private set; }

        public Announcements(ILogger<Announcements> logger)
        {
            _logger = logger;
            _announcements = new List<string>();
            AnnouncementsList = _announcements.AsReadOnly();
        }

        public void Add(string announcement)
        {
            _logger.LogInformation(announcement);
            _announcements.Add(announcement);
        }

        public void ReadAll()
        {
            // Don't need this right now, can just read logs.
        }
    }
}
