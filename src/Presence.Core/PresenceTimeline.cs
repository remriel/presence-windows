namespace Presence.Core;

public sealed record PresenceTimelineCell(DateOnly Date, TimeSpan Present, TimeSpan Recorded, TimeSpan Duration)
{
    public double? Fraction => Recorded == TimeSpan.Zero ? null : Math.Clamp(Present.TotalSeconds / Duration.TotalSeconds, 0, 1);
}

public sealed record PresenceTimelineRow(Device Device, IReadOnlyList<PresenceTimelineCell> Days);

public static class PresenceTimeline
{
    public static DateTimeOffset StartOfDay(DateOnly date, TimeZoneInfo? timeZone = null)
    {
        var zone = timeZone ?? TimeZoneInfo.Local;
        var local = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }

    public static List<PresenceTimelineRow> Build(IEnumerable<Device> devices, IEnumerable<PresenceEvent> events, DateTimeOffset from, DateTimeOffset through, TimeZoneInfo? timeZone = null)
    {
        if (from >= through) throw new ArgumentOutOfRangeException(nameof(from), "The timeline start must be earlier than its end.");
        var zone = timeZone ?? TimeZoneInfo.Local;
        var firstDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(from, zone).DateTime);
        var finalDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(through, zone).DateTime);
        var dates = Enumerable.Range(0, finalDay.DayNumber - firstDay.DayNumber + 1).Select(offset => firstDay.AddDays(offset)).ToArray();
        var dayStarts = dates.Select(date => StartOfDay(date, zone)).ToArray();
        var dayEnds = dates.Select(date => StartOfDay(date.AddDays(1), zone)).ToArray();
        var byMac = events.Where(e => e.Mac is not null && e.Type is "arrived" or "left")
            .GroupBy(e => e.Mac!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(e => e.At).ToArray(), StringComparer.OrdinalIgnoreCase);
        var rows = new List<PresenceTimelineRow>();

        foreach (var device in devices)
        {
            var present = new double[dates.Length];
            var recorded = new double[dates.Length];
            var cursor = through;
            bool? state = device.State switch
            {
                PresenceState.Home or PresenceState.ProbablyHome => true,
                PresenceState.Away => false,
                _ => null
            };
            var knownFrom = device.FirstSeen == default || device.FirstSeen < from ? from : device.FirstSeen;
            var transitions = byMac.GetValueOrDefault(device.Mac)?.ToList() ?? [];
            void AddTransition(DateTimeOffset? at, string type)
            {
                if (at is not { } timestamp || timestamp < from || timestamp > through ||
                    transitions.Any(item => item.At == timestamp && item.Type == type)) return;
                transitions.Add(PresenceEvent.Create(timestamp, type, device.DisplayName, device.Mac, device.PersonId));
            }

            if (device.State is PresenceState.Home or PresenceState.ProbablyHome)
                AddTransition(device.ChangedAt, "arrived");
            else if (device.State == PresenceState.Away)
            {
                AddTransition(device.ChangedAt, "left");
                if (device.ChangedAt is { } changedAt && device.LastSeen <= changedAt)
                    AddTransition(device.LastSeen, "arrived");
            }

            foreach (var item in transitions.OrderByDescending(item => item.At))
            {
                if (item.At < from || item.At > through) continue;
                Accumulate(item.At, cursor, state, knownFrom, dayStarts, dayEnds, present, recorded);
                state = item.Type == "arrived" ? false : true;
                cursor = item.At;
            }
            Accumulate(from, cursor, state, knownFrom, dayStarts, dayEnds, present, recorded);

            var cells = dates.Select((date, i) => new PresenceTimelineCell(
                date, TimeSpan.FromSeconds(present[i]), TimeSpan.FromSeconds(recorded[i]), dayEnds[i] - dayStarts[i])).ToArray();
            rows.Add(new(device, cells));
        }

        return rows;
    }

    private static void Accumulate(DateTimeOffset start, DateTimeOffset end, bool? state, DateTimeOffset knownFrom, DateTimeOffset[] dayStarts, DateTimeOffset[] dayEnds, double[] present, double[] recorded)
    {
        if (state is null || end <= start) return;
        start = start < knownFrom ? knownFrom : start;
        if (end <= start) return;
        for (var i = 0; i < dayStarts.Length; i++)
        {
            var overlapStart = start > dayStarts[i] ? start : dayStarts[i];
            var overlapEnd = end < dayEnds[i] ? end : dayEnds[i];
            if (overlapEnd <= overlapStart) continue;
            var seconds = (overlapEnd - overlapStart).TotalSeconds;
            recorded[i] += seconds;
            if (state.Value) present[i] += seconds;
        }
    }
}
