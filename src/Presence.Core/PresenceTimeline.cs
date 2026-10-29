namespace Presence.Core;

public sealed record PresenceTimelineCell(DateOnly Date, TimeSpan Present, TimeSpan Recorded, TimeSpan Duration)
{
    public double? Fraction => Recorded == TimeSpan.Zero ? null : Math.Clamp(Present.TotalSeconds / Duration.TotalSeconds, 0, 1);
}

public sealed record PresenceTimelineRow(Device Device, IReadOnlyList<PresenceTimelineCell> Days);

public sealed record PresenceHourlyTimelineCell(DateTimeOffset Start, DateTimeOffset End, TimeSpan Present, TimeSpan Recorded)
{
    public TimeSpan Duration => End - Start;
    public double? Fraction => Recorded == TimeSpan.Zero ? null : Math.Clamp(Present.TotalSeconds / Duration.TotalSeconds, 0, 1);
}

public sealed record PresenceHourlyTimelineRow(Device Device, IReadOnlyList<PresenceHourlyTimelineCell> Hours);

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
        var starts = dates.Select(date => StartOfDay(date, zone)).ToArray();
        var ends = dates.Select(date => StartOfDay(date.AddDays(1), zone)).ToArray();
        var estimates = Reconstruct(devices, events, from, through, starts, ends);
        return estimates.Select(estimate => new PresenceTimelineRow(
            estimate.Device,
            dates.Select((date, i) => new PresenceTimelineCell(
                date,
                TimeSpan.FromSeconds(estimate.Present[i]),
                TimeSpan.FromSeconds(estimate.Recorded[i]),
                ends[i] - starts[i])).ToArray())).ToList();
    }

    public static List<PresenceHourlyTimelineRow> BuildHourly(
        IEnumerable<Device> devices,
        IEnumerable<PresenceEvent> events,
        DateTimeOffset from,
        DateTimeOffset through)
    {
        var hourCount = (int)(through - from).TotalHours;
        if (through - from != TimeSpan.FromHours(hourCount) || hourCount is not (12 or 24))
            throw new ArgumentOutOfRangeException(nameof(from), "The hourly timeline must cover exactly 12 or 24 hours.");

        var first = from.ToUniversalTime();
        var starts = Enumerable.Range(0, hourCount).Select(hour => first.AddHours(hour)).ToArray();
        var ends = Enumerable.Range(1, hourCount).Select(hour => first.AddHours(hour)).ToArray();
        var estimates = Reconstruct(devices, events, first, through.ToUniversalTime(), starts, ends);
        return estimates.Select(estimate => new PresenceHourlyTimelineRow(
            estimate.Device,
            Enumerable.Range(0, hourCount).Select(hour => new PresenceHourlyTimelineCell(
                starts[hour],
                ends[hour],
                TimeSpan.FromSeconds(estimate.Present[hour]),
                TimeSpan.FromSeconds(estimate.Recorded[hour]))).ToArray())).ToList();
    }

    private static List<Estimate> Reconstruct(
        IEnumerable<Device> devices,
        IEnumerable<PresenceEvent> events,
        DateTimeOffset from,
        DateTimeOffset through,
        DateTimeOffset[] intervalStarts,
        DateTimeOffset[] intervalEnds)
    {
        var byMac = events.Where(e => e.Mac is not null && e.Type is "arrived" or "left")
            .GroupBy(e => e.Mac!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(e => e.At).ToArray(), StringComparer.OrdinalIgnoreCase);
        var estimates = new List<Estimate>();

        foreach (var device in devices)
        {
            var present = new double[intervalStarts.Length];
            var recorded = new double[intervalStarts.Length];
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
                Accumulate(item.At, cursor, state, knownFrom, intervalStarts, intervalEnds, present, recorded);
                state = item.Type == "arrived" ? false : true;
                cursor = item.At;
            }
            Accumulate(from, cursor, state, knownFrom, intervalStarts, intervalEnds, present, recorded);
            estimates.Add(new(device, present, recorded));
        }

        return estimates;
    }

    private static void Accumulate(DateTimeOffset start, DateTimeOffset end, bool? state, DateTimeOffset knownFrom, DateTimeOffset[] intervalStarts, DateTimeOffset[] intervalEnds, double[] present, double[] recorded)
    {
        if (state is null || end <= start) return;
        start = start < knownFrom ? knownFrom : start;
        if (end <= start) return;
        for (var i = 0; i < intervalStarts.Length; i++)
        {
            var overlapStart = start > intervalStarts[i] ? start : intervalStarts[i];
            var overlapEnd = end < intervalEnds[i] ? end : intervalEnds[i];
            if (overlapEnd <= overlapStart) continue;
            var seconds = (overlapEnd - overlapStart).TotalSeconds;
            recorded[i] += seconds;
            if (state.Value) present[i] += seconds;
        }
    }

    private sealed record Estimate(Device Device, double[] Present, double[] Recorded);
}
