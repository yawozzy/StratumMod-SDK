using Stratum;

namespace ExampleMod;

// Messages between players in the same online room who have this mod.
sealed class SyncExamples
{
    readonly StratumMod mod;
    readonly List<string> inbox = new();
    string message = "";

    public SyncExamples(StratumMod mod) => this.mod = mod;

    public void Register()
    {
        // Text messages.
        mod.Sync.On("chat", m => Add(m.Sender.Name + ": " + m.Text));

        // Packed values: read them in the order they were written.
        mod.Sync.On("ping", m =>
        {
            var reader = m.Read();
            int number = reader.ReadInt();
            var (x, y, z) = reader.ReadVector();
            Add($"{m.Sender.Name} sent ping #{number} at ({x:0}, {y:0}, {z:0})");
        });

        mod.Sync.PlayerJoined += p => Add(p.Name + " has Example Mod");
        mod.Sync.PlayerLeft += p => Add(p.Name + " left");
    }

    void Add(string line)
    {
        inbox.Add(line);
        if (inbox.Count > 8) inbox.RemoveAt(0);
        mod.Log.Info("Sync: " + line);
    }

    public void Draw()
    {
        var ui = mod.UI;
        var sync = mod.Sync;
        if (!sync.IsAvailable)
        {
            ui.Label("Join an online room to try sync.");
            ui.Hint("Only players who also have this mod receive the messages.");
            return;
        }

        ui.Label((sync.IsPrivateRoom ? "Private room" : "Public room") + ", players with this mod: " + sync.Players.Count);
        foreach (var p in sync.Players) ui.Hint("• " + p.Name);

        ui.Input("Message", ref message, 64);
        using (ui.Row())
        {
            if (ui.Button("Send", primary: true) && message.Length > 0 && sync.Send("chat", message)) message = "";
            if (ui.Button("Send ping"))
                sync.Send("ping", new SyncWriter().Write(inboxCounter++).Write(1f, 2f, 3f));
        }

        using (ui.Group("Received"))
        {
            if (inbox.Count == 0) ui.Hint("Nothing yet.");
            foreach (var line in inbox) ui.Label(line);
        }
    }

    int inboxCounter = 1;
}
