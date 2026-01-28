namespace Gymunity.Admin.MVC.ViewModels.Notifications
{
    public class NotificationsListViewModel
    {
        public IEnumerable<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }

        /// <summary>
        /// Categorized notifications grouped by type
        /// </summary>
        public Dictionary<string, List<NotificationViewModel>> GroupedByCategory
        {
            get
            {
                return Notifications
                    .GroupBy(n => n.NotificationCategory)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(n => n.CreatedAt).ToList()
                    );
            }
        }

        /// <summary>
        /// Get statistics by category
        /// </summary>
        public Dictionary<string, int> CategoryCounts
        {
            get
            {
                return Notifications
                    .GroupBy(n => n.NotificationCategory)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        }

        /// <summary>
        /// Get high-priority notifications count
        /// </summary>
        public int HighPriorityCount
        {
            get
            {
                return Notifications.Count(n => n.IsHighPriority);
            }
        }

        /// <summary>
        /// Get unread high-priority notifications
        /// </summary>
        public int UnreadHighPriorityCount
        {
            get
            {
                return Notifications.Count(n => n.IsHighPriority && !n.IsRead);
            }
        }
    }
}
