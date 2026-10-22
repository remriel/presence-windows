using System.Text.Json;
using Presence.App;
using Presence.Core;

internal static partial class Program
{
    private static void Click(Control form, string name) => All(form).OfType<Button>().Single(button => button.AccessibleName == name).PerformClick();

    private static int DeviceCategoryChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "CategoryPreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true);
        app.Engine.Data.Devices.Clear(); app.Engine.Data.People.Clear();
        var person = new Person { Id = "fictional-alex", Name = "Alex", State = PresenceState.Home };
        app.Engine.Data.People.Add(person);
        var samples = new[]
        {
            ("Alex's phone", DeviceCategory.Phone), ("Work laptop", DeviceCategory.Laptop),
            ("Studio desktop", DeviceCategory.Desktop), ("Reading tablet", DeviceCategory.Tablet),
            ("Living room TV", DeviceCategory.TvStreaming), ("Office printer", DeviceCategory.Printer),
            ("Music speaker", DeviceCategory.Speaker), ("Home router", DeviceCategory.NetworkEquipment)
        };
        for (var i = 0; i < samples.Length; i++)
            app.Engine.Data.Devices.Add(new Device
            {
                Mac = $"02:00:00:00:00:{i + 1:X2}", Name = samples[i].Item1, Category = samples[i].Item2,
                Ip = $"192.0.2.{i + 10}", Hostname = i == 0 ? "Alex-iPhone" : "", Network = "demo",
                Kind = i == 0 ? DeviceKind.Person : DeviceKind.Known, PersonId = i == 0 ? person.Id : null,
                IsPrimary = i == 0, FirstSeen = DateTimeOffset.Now.AddDays(-2), LastSeen = DateTimeOffset.Now,
                State = i == 3 ? PresenceState.Away : PresenceState.Home, ChangedAt = DateTimeOffset.Now.AddMinutes(-15)
            });
        app.Store.Save(app.Engine.Data);
        var target = app.Engine.Data.Devices[0];
        var main = GetMain(app); main.Show(); Application.DoEvents();
        foreach (var theme in new[] { "Light", "Dark" })
        {
            var settings = app.Engine.Data.Settings; settings.Theme = theme; app.Refresh();
            var prefix = "categories-" + theme.ToLowerInvariant();
            using (var form = new DeviceWindow(app, target))
            {
                form.Show(); Application.DoEvents();
                var picker = All(form).OfType<CategoryPicker>().Single();
                var choices = All(picker).OfType<CategoryChoice>().ToList();
                Check(choices.Count(c => c.Checked) == 1 && picker.SelectedCategory == DeviceCategory.Phone, theme + " saved category opens checked");
                foreach (var choice in choices)
                {
                    choice.Checked = true;
                    Check(choices.Count(c => c.Checked) == 1 && picker.SelectedCategory == choice.Category, theme + " mutually exclusive " + choice.Category);
                    Check(choice.TabStop && choice.AccessibilityObject.Role == AccessibleRole.CheckButton, theme + " keyboard and checkbox accessibility " + choice.Category);
                    var (ink, fill) = CategoryAppearance.Colors(choice.Category, settings);
                    Check(Contrast(ink, fill) >= 4.5 && Contrast(ink, Ui.Surface(settings)) >= 4.5, theme + " category text contrast " + choice.Category);
                }
                var phone = choices.Single(c => c.Category == DeviceCategory.Phone);
                phone.Checked = true; phone.Checked = false;
                Check(picker.SelectedCategory == DeviceCategory.Automatic && choices.Single(c => c.Category == DeviceCategory.Automatic).Checked,
                    theme + " unchecking category returns to Automatic");
                choices.Single(c => c.Category == DeviceCategory.Laptop).Checked = true;
                CaptureExisting(form, prefix + "-editor", form.DeviceDpi / 96f);
                CheckCategoryFit(form, prefix);
                form.Size = form.MinimumSize; Application.DoEvents();
                CheckCategoryFit(form, prefix + "-minimum");
                CaptureExisting(form, prefix + "-editor-minimum", form.DeviceDpi / 96f);
                Click(form, "Cancel");
                Check(target.Category == DeviceCategory.Phone && app.Store.Load().Devices[0].Category == DeviceCategory.Phone,
                    theme + " Cancel preserves live and persisted category");
            }
            using (var inventory = new DevicesWindow(app))
            {
                inventory.Show(); Application.DoEvents();
                var grid = All(inventory).OfType<DataGridView>().Single();
                Check(grid.Rows[0].Cells["Category"].Value?.ToString() == "Phone", theme + " inventory displays category labels");
                Check(grid.Rows[0].Cells["Category"].Style.SelectionBackColor == CategoryAppearance.Colors(DeviceCategory.Phone, settings).Fill,
                    theme + " selected inventory cells retain category color");
                CaptureExisting(inventory, prefix + "-inventory", inventory.DeviceDpi / 96f);
            }
            CaptureExisting(main, prefix + "-home", main.DeviceDpi / 96f);
            using (var timeline = new PresenceTimelineWindow(app))
            {
                timeline.Show(); Application.DoEvents();
                var grid = All(timeline).OfType<DataGridView>().Single();
                Check(grid.Rows.Cast<DataGridViewRow>().Single(r => ReferenceEquals(r.Tag, target)).Cells[0].Style.BackColor == CategoryAppearance.Colors(target.EffectiveCategory, settings).Fill,
                    theme + " daily timeline labels use category color");
                Click(timeline, "24 hours"); Application.DoEvents();
                grid = All(timeline).OfType<DataGridView>().Single();
                Check(grid.Rows.Cast<DataGridViewRow>().Single(r => ReferenceEquals(r.Tag, target)).Cells[0].ToolTipText.Contains("Phone"),
                    theme + " hourly timeline tooltip includes category");
                CaptureExisting(timeline, prefix + "-timeline", timeline.DeviceDpi / 96f);
            }
        }

        using var db = new Microsoft.Data.Sqlite.SqliteConnection(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = app.Store.Path }.ToString());
        db.Open(); using var command = db.CreateCommand();
        command.CommandText = "CREATE TRIGGER RejectCategorySave BEFORE UPDATE ON State BEGIN SELECT RAISE(ABORT,'fixture save failure'); END"; command.ExecuteNonQuery();
        using (var form = new DeviceWindow(app, target))
        {
            form.Show(); Application.DoEvents();
            All(form).OfType<CategoryChoice>().Single(c => c.Category == DeviceCategory.Laptop).Checked = true;
            Click(form, "Save");
            Check(!form.IsDisposed && target.Category == DeviceCategory.Phone && target.IsPrimary && target.PersonId == person.Id,
                "Failed save restores category and retains primary/person association");
            Check(app.Store.Load().Devices[0].Category == DeviceCategory.Phone && All(form).OfType<WrapLabel>().Any(c => c.Visible && c.Text.StartsWith("Could not save")),
                "Failed save preserves disk state and shows a retryable error");
            command.CommandText = "DROP TRIGGER RejectCategorySave"; command.ExecuteNonQuery();
            Click(form, "Save");
            Check(form.IsDisposed && target.Category == DeviceCategory.Laptop && app.Store.Load().Devices[0].Category == DeviceCategory.Laptop,
                "Retry commits the checked category");
        }
        var homeCell = All(main).OfType<DataGridView>().SelectMany(g => g.Rows.Cast<DataGridViewRow>())
            .Single(r => Equals(r.Tag, target.Mac)).Cells["When"];
        Check(homeCell.Value?.ToString()?.Contains("Laptop") == true && homeCell.Style.BackColor == CategoryAppearance.Colors(DeviceCategory.Laptop, app.Engine.Data.Settings).Fill,
            "Saving only a category refreshes the visible home label and color");
        using (var reopened = new DeviceWindow(app, target))
        {
            reopened.Show(); Application.DoEvents();
            Check(All(reopened).OfType<CategoryChoice>().Single(c => c.Checked).Category == DeviceCategory.Laptop, "Reopening the editor checks the saved choice");
            All(reopened).OfType<CategoryChoice>().Single(c => c.Category == DeviceCategory.Automatic).Checked = true;
            Click(reopened, "Save");
            Check(target.Category == DeviceCategory.Automatic && target.DeviceType == "Phone" && app.Store.Load().Devices[0].Category == DeviceCategory.Automatic,
                "Saving Automatic restores detection and persists the fallback");
        }
        app.SaveDevice(target, target.Name, target.Kind, person.Name, target.IsPrimary, DeviceCategory.Laptop);
        app.SetDeviceKinds([target.Mac], DeviceKind.Ignore);
        Check(target.Category == DeviceCategory.Laptop && app.Store.Load().Devices[0].Category == DeviceCategory.Laptop, "Bulk tracking changes retain physical category");
        app.SetDeviceKinds([target.Mac], DeviceKind.Known);
        app.SaveDevice(target, target.Name, DeviceKind.Known, "", false);
        Check(target.Category == DeviceCategory.Laptop, "Existing tracking-only callers retain the saved category");
        main.AllowClose = true; main.Close();
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { checks, failures, captures }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Device categories: {checks.Count} checks; {failures.Count} findings; {captures.Count} captures.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }

    private static void CheckCategoryFit(Form form, string name)
    {
        foreach (var choice in All(form).OfType<CategoryChoice>())
        {
            var text = TextRenderer.MeasureText(choice.Text, choice.Font);
            if (text.Width + choice.Padding.Horizontal + 24 * form.DeviceDpi / 96f > choice.ClientSize.Width)
                failures.Add(name + ": clipped category checkbox " + choice.Text);
        }
    }

    private static double Contrast(Color a, Color b)
    {
        static double Luminance(Color c)
        {
            static double Linear(byte component) { var value = component / 255d; return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4); }
            return .2126 * Linear(c.R) + .7152 * Linear(c.G) + .0722 * Linear(c.B);
        }
        var first = Luminance(a); var second = Luminance(b);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }
}
