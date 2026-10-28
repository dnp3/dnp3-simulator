### Graphical DNP3 Master/Outstation simulator

Depends on opendnp3 via NuGet:

[https://www.nuget.org/packages/opendnp3](https://www.nuget.org/packages/opendnp3)

### Configuration and node settings

- **File > Save** (`Ctrl+S`) exports the current configuration to an XML file.
- **File > Load** (`Ctrl+O`) replaces the current session with a saved configuration.
- Closing the application saves the session to `%LOCALAPPDATA%\Automatak\DNP3Simulator\session.xml`; the next launch restores it automatically.
- Right-click a channel, master, or outstation and choose **Settings...** to edit its current configuration. **Apply** restarts that node; changing a channel restarts its children too. **Cancel** leaves the running configuration unchanged. **Open** still opens the master/outstation operational window.

Saved configuration includes channel/TLS/serial settings, master and outstation parameters, database templates, periodic master scans, tree hierarchy, expanded nodes, and selection. Live measurements, event history, and open operational windows are not session settings and are not saved. TLS configuration stores certificate/key paths, not their contents.

Files are validated before the current tree is replaced. If a native node cannot be created, the previous configuration is restored. An unreadable automatic session file is preserved; successful **Load** or **Save** re-enables automatic saving.

Run `tests\Run-ConfigurationSmokeTests.ps1` to build the x86 application and check XML round trips, dialog settings, native node restoration, session persistence, and rollback. Pass `-MSBuild <path>` if MSBuild is installed elsewhere.
