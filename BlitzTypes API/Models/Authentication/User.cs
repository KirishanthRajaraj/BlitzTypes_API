using BlitzTypes_API.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace BlitzTypes_API.Models.Authentication
{
    public class User : IdentityUser
    {
        public DateTime? joinedDate { get; set; }
        public string? BlitztypesTitle { get; set; }
        public TimeSpan? typingTime { get; set; }
        public int? highScoreWPM_15_sec { get; set; }
        public int? highScoreWPM_30_sec { get; set; }
        public int? highScoreWPM_60_sec { get; set; }
        public int? highScoreAccuracy { get; set; }
        public int? secondsWritten { get; set; }
        public int? testAmount { get; set; }
        public Language? preferredLanguage { get; set; }
        public Language? preferredTime { get; set; }
        public string? refreshTokenHash { get; set; }
        public DateTime? refreshTokenExpiry { get; set; }
    }
}
