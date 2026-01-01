using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI
{
    internal sealed class SelectArchetypesForm : Form
    {
        private readonly List<string> _all;
        private readonly HashSet<string> _checked = new(StringComparer.OrdinalIgnoreCase);

        private Label _lblFilter = null!;
        private TextBox _txtFilter = null!;
        private CheckedListBox _list = null!;
        private Label _lblCount = null!;
        private Button _btnAdd = null!;
        private Button _btnCancel = null!;
        private Button _btnSelectAll = null!;
        private Button _btnClear = null!;

        public IReadOnlyList<string> SelectedArchetypes { get; private set; } = Array.Empty<string>();

        public SelectArchetypesForm(IReadOnlyList<string> archetypeNames)
        {
            _all = (archetypeNames ?? Array.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            InitializeUi();
            ApplyTheme(SettingsManager.Current.Theme);
            Shown += (_, __) => ApplyTheme(SettingsManager.Current.Theme);
            ApplyFilter();
        }

        private void InitializeUi()
        {
            Text = "Select Archetypes";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 560);
            MinimumSize = new Size(560, 420);

            _lblFilter = new Label
            {
                Text = "Filter:",
                AutoSize = true,
                Location = new Point(12, 14)
            };

            _txtFilter = new TextBox
            {
                Location = new Point(60, 10),
                Size = new Size(420, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _txtFilter.TextChanged += (_, __) => ApplyFilter();

            _btnSelectAll = new Button
            {
                Text = "Select All",
                Location = new Point(490, 9),
                Size = new Size(70, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnSelectAll.Click += (_, __) => SelectAllVisible(true);

            _btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(565, 9),
                Size = new Size(60, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnClear.Click += (_, __) => SelectAllVisible(false);

            _list = new CheckedListBox
            {
                Location = new Point(12, 42),
                Size = new Size(613, 440),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                CheckOnClick = true
            };
            _list.ItemCheck += (_, __) => BeginInvoke(new Action(CaptureCheckedFromList));

            _lblCount = new Label
            {
                Location = new Point(12, 488),
                Size = new Size(420, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnAdd = new Button
            {
                Text = "Add Selected",
                Location = new Point(430, 512),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _btnAdd.Click += (_, __) => ConfirmSelection();

            _btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(545, 512),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            Controls.Add(_lblFilter);
            Controls.Add(_txtFilter);
            Controls.Add(_btnSelectAll);
            Controls.Add(_btnClear);
            Controls.Add(_list);
            Controls.Add(_lblCount);
            Controls.Add(_btnAdd);
            Controls.Add(_btnCancel);

            AcceptButton = _btnAdd;
            CancelButton = _btnCancel;
        }

        private void ConfirmSelection()
        {
            CaptureCheckedFromList();

            SelectedArchetypes = _checked
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CaptureCheckedFromList()
        {
            if (_list == null)
                return;

            foreach (var item in _list.Items)
            {
                if (item is not string s)
                    continue;

                bool isChecked = _list.CheckedItems.Contains(item);
                if (isChecked)
                    _checked.Add(s);
                else
                    _checked.Remove(s);
            }

            UpdateCountLabel();
        }

        private void SelectAllVisible(bool check)
        {
            CaptureCheckedFromList();

            _list.BeginUpdate();
            try
            {
                for (int i = 0; i < _list.Items.Count; i++)
                {
                    _list.SetItemChecked(i, check);

                    if (_list.Items[i] is string s)
                    {
                        if (check)
                            _checked.Add(s);
                        else
                            _checked.Remove(s);
                    }
                }
            }
            finally
            {
                _list.EndUpdate();
            }

            UpdateCountLabel();
        }

        private void ApplyFilter()
        {
            CaptureCheckedFromList();

            string filter = (_txtFilter.Text ?? string.Empty).Trim();
            var tokens = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToArray();

            IEnumerable<string> filtered = _all;
            if (tokens.Length > 0)
            {
                filtered = _all.Where(s =>
                {
                    foreach (var tok in tokens)
                    {
                        if (s.IndexOf(tok, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }
                    return true;
                });
            }

            var list = filtered.ToList();

            _list.BeginUpdate();
            try
            {
                _list.Items.Clear();
                foreach (var s in list)
                {
                    int idx = _list.Items.Add(s);
                    if (_checked.Contains(s))
                        _list.SetItemChecked(idx, true);
                }
            }
            finally
            {
                _list.EndUpdate();
            }

            UpdateCountLabel(list.Count);
        }

        private void UpdateCountLabel(int? visibleCount = null)
        {
            int visible = visibleCount ?? _list.Items.Count;
            _lblCount.Text = $"Visible: {visible}    Selected: {_checked.Count}    Total: {_all.Count}";
        }

        private void ApplyTheme(AppTheme theme)
        {
            ThemePalette palette = ThemeHelper.GetPalette(theme);

            BackColor = palette.WindowBack;
            ForeColor = palette.TextColor;

            if (_lblFilter != null)
            {
                _lblFilter.ForeColor = palette.TextColor;
                _lblFilter.BackColor = palette.WindowBack;
            }

            if (_txtFilter != null)
            {
                _txtFilter.BackColor = palette.InputBack;
                _txtFilter.ForeColor = palette.TextColor;
                _txtFilter.BorderStyle = BorderStyle.FixedSingle;
            }

            if (_list != null)
            {
                _list.BackColor = palette.LogBack;
                _list.ForeColor = palette.LogText;
                _list.BorderStyle = BorderStyle.FixedSingle;
            }

            if (_lblCount != null)
            {
                _lblCount.ForeColor = palette.AccentColor;
                _lblCount.BackColor = palette.WindowBack;
            }

            // Primary button
            if (_btnAdd != null)
            {
                _btnAdd.UseVisualStyleBackColor = false;
                _btnAdd.BackColor = palette.AccentColor;
                _btnAdd.ForeColor = Color.White;
                _btnAdd.FlatStyle = FlatStyle.Flat;
                _btnAdd.FlatAppearance.BorderColor = palette.BorderColor;
            }

            // Secondary buttons
            Button[] secondaryButtons =
            {
                _btnCancel,
                _btnSelectAll,
                _btnClear
            };

            foreach (var btn in secondaryButtons)
            {
                if (btn == null) continue;
                btn.UseVisualStyleBackColor = false;
                btn.BackColor = palette.SecondaryButton;
                btn.ForeColor = palette.TextColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = palette.BorderColor;
            }
        }
    }
}
