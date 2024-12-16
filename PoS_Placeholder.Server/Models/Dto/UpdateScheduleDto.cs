using PoS_Placeholder.Server.Utilities;
using System.Text.Json.Serialization;

namespace PoS_Placeholder.Server.Models.Dto
{
    public class UpdateScheduleDto
    {
        public DateTime? Day { get; set; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly? StartTime { get; set; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly? EndTime { get; set; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly? BreakStart { get; set; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly? BreakEnd { get; set; }
    }

}
