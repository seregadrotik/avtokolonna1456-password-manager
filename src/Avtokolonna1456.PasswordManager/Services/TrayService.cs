using System;
using System.Drawing;
using System.Windows.Forms;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class TrayService : IDisposable
    {
        private NotifyIcon _icon;

        public event Action OpenRequested;
        public event Action CheckRemindersRequested;
        public event Action ExitRequested;

        public void Initialize()
        {
            if (_icon != null) return;

            _icon = new NotifyIcon
            {
                Text = "Автоколонна 1456 — Менеджер паролей",
                Visible = true,
                Icon = LoadIcon()
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Открыть", null, (s, e) => OpenRequested?.Invoke());
            menu.Items.Add("Проверить напоминания", null, (s, e) => CheckRemindersRequested?.Invoke());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Выход", null, (s, e) => ExitRequested?.Invoke());
            _icon.ContextMenuStrip = menu;
            _icon.DoubleClick += (s, e) => OpenRequested?.Invoke();
        }

        public void ShowBalloon(string text)
        {
            if (_icon == null) return;
            _icon.BalloonTipTitle = "Автоколонна 1456";
            _icon.BalloonTipText = text;
            _icon.ShowBalloonTip(4000);
        }

        public void Dispose()
        {
            if (_icon == null) return;
            _icon.Visible = false;
            _icon.Dispose();
            _icon = null;
        }

        private static Icon LoadIcon()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Resources/app.ico");
                var streamInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamInfo != null)
                {
                    return new Icon(streamInfo.Stream);
                }
            }
            catch
            {
            }

            return SystemIcons.Application;
        }
    }
}
