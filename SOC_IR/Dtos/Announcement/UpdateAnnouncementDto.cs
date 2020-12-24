using System;

namespace SOC_IR.Dtos.Announcement
{
    public class UpdateAnnouncementDto
    {
        public string announceID { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string description { get; set; }
        public Boolean isImportant { get; set; }
        public Boolean isActive { get; set; }
        public string validTill { get; set; }
        public string announcedBy { get; set; }
    }
}