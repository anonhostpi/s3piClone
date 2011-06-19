/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    internal partial class CopyableMessageBoxInternal : Form
    {
        internal Button theButton = null;
        private CopyableMessageBoxInternal()
        {
            InitializeComponent();
        }

        Label lb = new Label() { AutoSize = true, Margin = new Padding(12), };
        internal CopyableMessageBoxInternal(string message, string caption, CopyableMessageBoxIcon icon, IList<string> buttons, int defBtn, int cncBtn)
            : this()
        {
            if (buttons.Count < 1)
                throw new ArgumentLengthException("At least one button text must be supplied");

            this.SuspendLayout();


            int formWidth = Bounds.Width - ClientSize.Width; // screen estate used by the form border
            int formHeight = Bounds.Height - ClientSize.Height; // screen estate used by the form border
            int tbPadding = tbMessage.Margin.Left + tbMessage.Margin.Right; // screen estate used by the text box regardless of content
            int buttonHeight = flpButtons.Height; // screen estate reserved for the buttons

            int iconWidth = icon == CopyableMessageBoxIcon.None ? 0 : 80; // icon area, if icon present
            int iconHeight = icon == CopyableMessageBoxIcon.None ? 0 : 77; // icon area, if icon present

            // To calculate the text box size, we get an autosize label to tell us how big it should be
            Size winSize = CopyableMessageBox.OwningForm != null ? CopyableMessageBox.OwningForm.Size : Screen.PrimaryScreen.WorkingArea.Size;
            if (winSize.Width < Screen.PrimaryScreen.WorkingArea.Size.Width / 4) winSize.Width = Screen.PrimaryScreen.WorkingArea.Size.Width;
            if (winSize.Height < Screen.PrimaryScreen.WorkingArea.Size.Height / 4) winSize.Height = Screen.PrimaryScreen.WorkingArea.Size.Height;
            lb.MaximumSize = new Size((int)(winSize.Width * .8) - (formWidth + tbPadding + iconWidth),
                (int)(winSize.Height * .8) - (formHeight + buttonHeight + tbPadding));
            lb.Text = message;

            tbMessage_SizeChanged(tbMessage, null);

            int buttonWidth = 15 + (81 * (buttons.Count - 1)) + 75 + 15;
            int textWidth = tbPadding + lb.PreferredWidth;
            int textHeight = tbPadding + lb.PreferredHeight;

            this.ClientSize = new Size(Math.Max(buttonWidth, iconWidth + textWidth),
                buttonHeight + Math.Max(iconHeight, textHeight));


            this.Text = caption;

            this.tbMessage.Lines = message.Split('\n');

            enumToGlyph(icon, lbIcon);

            CreateButtons(buttons, defBtn, cncBtn);


            this.ResumeLayout();

            this.DialogResult = DialogResult.OK;
        }

        private void enumToGlyph(CopyableMessageBoxIcon icon, Label lb)
        {
            switch (icon)
            {
                case CopyableMessageBoxIcon.Information:
                    lb.Visible = true; lb.Text = "i"; lb.ForeColor = Color.Blue; lb.BackColor = Color.FromArgb(240, 240, 255); lb.Font = new Font(lb.Font, FontStyle.Italic); break;
                case CopyableMessageBoxIcon.Question:
                    lb.Visible = true; lb.Text = "?"; lb.ForeColor = Color.Green; lb.BackColor = Color.FromArgb(240, 255, 240); lb.Font = new Font(lb.Font, FontStyle.Regular); break;
                case CopyableMessageBoxIcon.Warning:
                    lb.Visible = true; lb.Text = "!"; lb.ForeColor = Color.Black; lb.BackColor = Color.Yellow; lb.Font = new Font(lb.Font, FontStyle.Bold); break;
                case CopyableMessageBoxIcon.Error:
                    lb.Visible = true; lb.Text = "X"; lb.ForeColor = Color.White; lb.BackColor = Color.Red; lb.Font = new Font(lb.Font, FontStyle.Bold); break;
                case CopyableMessageBoxIcon.None:
                default:
                    lb.Visible = false; break;
            }
        }

        private void CreateButtons(IList<string> buttons, int defBtn, int cncBtn)
        {
            flpButtons.SuspendLayout();
            flpButtons.Controls.Clear();
            for (int i = buttons.Count; i > 0; i--)
            {
                Button btn = CreateButton("button" + i, i, buttons[i - 1]);
                flpButtons.Controls.Add(btn);
                if (i == defBtn + 1) this.AcceptButton = btn;
                if (i == cncBtn + 1) this.CancelButton = btn;
            }
            flpButtons.ResumeLayout();
        }

        private Button CreateButton(string Name, int TabIndex, string Text)
        {
            Button newButton = new Button();
            newButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            newButton.Name = Name;
            newButton.Size = new System.Drawing.Size(75, 23);
            newButton.TabIndex = TabIndex;
            newButton.Text = Text;
            newButton.UseVisualStyleBackColor = true;
            newButton.Click += new System.EventHandler(this.button_Click);
            return newButton;
        }

        private void button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;//To avoid it becoming Cancel
            theButton = sender as Button;
            this.Close();
        }

        private void tbMessage_SizeChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            tbMessage.ScrollBars = ((tb.Height < lb.PreferredHeight || tb.Width < lb.PreferredWidth) ? ScrollBars.Vertical : ScrollBars.None);
        }
    }

    /// <summary>
    /// Specifies constants defining which symbol to display on a <see cref="CopyableMessageBox"/>.
    /// </summary>
    public enum CopyableMessageBoxIcon
    {
        /// <summary>
        /// The message box contain no symbols.
        /// </summary>
        /// <remarks>This is the default for unknown values.</remarks>
        None = 0,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i.
        /// This is styled italic and displayed in blue on very pale blue.
        /// </summary>
        Information = 1,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i.
        /// This is styled italic and displayed in blue on very pale blue.
        /// </summary>
        /// <remarks>This is the same as <see cref="CopyableMessageBoxIcon.Information"/>.</remarks>
        Asterisk = 1,
        /// <summary>
        /// The message box contains a symbol consisting of a question mark.
        /// This is styled regular and displayed in green on very pale green.
        /// </summary>
        Question = 2,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation mark.
        /// This is styled bold and displayed in black on yellow.
        /// </summary>
        Warning = 3,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation mark.
        /// This is styled bold and displayed in black on yellow.
        /// </summary>
        /// <remarks>This is the same as <see cref="CopyableMessageBoxIcon.Warning"/>.</remarks>
        Exclamation = 3,
        /// <summary>
        /// The message box contains a symbol consisting of an uppercase letter X.
        /// This is styled bold and displayed in white on red.
        /// </summary>
        Error = 4,
        /// <summary>
        /// The message box contains a symbol consisting of an uppercase letter X.
        /// This is styled bold and displayed in white on red.
        /// </summary>
        /// <remarks>This is the same as <see cref="CopyableMessageBoxIcon.Error"/>.</remarks>
        Hand = 4,
        /// <summary>
        /// The message box contains a symbol consisting of an uppercase letter X.
        /// This is styled bold and displayed in white on red.
        /// </summary>
        /// <remarks>This is the same as <see cref="CopyableMessageBoxIcon.Error"/>.</remarks>
        Stop = 4,
    }

    /// <summary>
    /// Specifies constants defining which buttons to display on a <see cref="CopyableMessageBox"/>.
    /// </summary>
    public enum CopyableMessageBoxButtons
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        /// <remarks>This is the default for unknown values.</remarks>
        OK,
        /// <summary>
        /// The message box contains OK and Cancel buttons.
        /// </summary>
        OKCancel,
        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore,
        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel,
        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo,
        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel,
    }

    /// <summary>
    /// Displays a message box from which the text can be copied.
    /// </summary>
    public static class CopyableMessageBox
    {
        internal static Form OwningForm
        {
            get
            {
                Form owner = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                if (owner != null && (owner.InvokeRequired || owner.IsDisposed || !owner.IsHandleCreated)) owner = null;
                return owner;
            }
        }

        /// <summary>
        /// Displays a message box with the specified text.
        /// The text can be copied.
        /// </summary>
        /// <param name="message">The text to display in the message box.  This text can be copied.</param>
        /// <returns>Always returns <c>0</c>.</returns>
        public static int Show(string message)
        {
            return Show(message, OwningForm != null ? OwningForm.Text : Application.ProductName,
                CopyableMessageBoxIcon.None, new List<string>(new string[] { "OK" }), 0, 0);
        }
        /// <summary>
        /// Displays a message box with the specified text, caption, buttons, icon and default button.
        /// The text can be copied.
        /// </summary>
        /// <param name="message">The text to display in the message box.  This text can be copied.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the <see cref="CopyableMessageBoxButtons"/> values that specifies which buttons to display in the message box.
        /// If not specified, <see cref="CopyableMessageBoxButtons.OK"/> is used.</param>
        /// <param name="icon">One of the <see cref="CopyableMessageBoxIcon"/> values that specifies which icon to display in the message box.
        /// If not specified, <see cref="CopyableMessageBoxIcon.None"/> is used.</param>
        /// <param name="defBtn">The zero-based index of the default button.
        /// If not specified, <c>0</c> is used.</param>
        /// <returns>The zero-based index of the button pressed.</returns>
        public static int Show(string message, string caption,
            CopyableMessageBoxButtons buttons = CopyableMessageBoxButtons.OK,
            CopyableMessageBoxIcon icon = CopyableMessageBoxIcon.None,
            int defBtn = 0)
        {
            int cncBtn = enumToCncBtn(buttons);
            return Show(message, caption, icon, enumToList(buttons), defBtn, cncBtn);
        }
        /// <summary>
        /// Displays a message box with the specified text, caption, icon, buttons, default button and cancel button.
        /// The text can be copied.
        /// </summary>
        /// <param name="message">The text to display in the message box.  This text can be copied.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the <see cref="CopyableMessageBoxButtons"/> values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the <see cref="CopyableMessageBoxIcon"/> values that specifies which icon to display in the message box.</param>
        /// <param name="defBtn">The zero-based index of the default button.</param>
        /// <param name="cncBtn">The zero-based index of the cancel button.</param>
        /// <returns>The zero-based index of the button pressed.</returns>
        public static int Show(string message, string caption, CopyableMessageBoxButtons buttons, CopyableMessageBoxIcon icon, int defBtn, int cncBtn)
        {
            return Show(message, caption, icon, enumToList(buttons), defBtn, cncBtn);
        }

        /// <summary>
        /// Displays a message box with the specified text, caption, icon, buttons, default button and cancel button.
        /// The text can be copied.
        /// </summary>
        /// <param name="message">The text to display in the message box.  This text can be copied.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="icon">One of the <see cref="CopyableMessageBoxIcon"/> values that specifies which icon to display in the message box.</param>
        /// <param name="buttons">A <see cref="IList{T}"/> (where <c>T</c> is <see cref="string"/>) to display as buttons in the message box.</param>
        /// <param name="defBtn">The zero-based index of the default button.</param>
        /// <param name="cncBtn">The zero-based index of the cancel button.</param>
        /// <returns>The zero-based index of the button pressed.</returns>
        public static int Show(string message, string caption, CopyableMessageBoxIcon icon, IList<string> buttons, int defBtn, int cncBtn)
        {
            return Show(OwningForm, message, caption, icon, buttons, defBtn, cncBtn);
        }

        /// <summary>
        /// Displays a message box with the specified text, caption, icon, buttons, default button and cancel button.
        /// The text can be copied.
        /// </summary>
        /// <param name="owner">An implementation of <see cref="System.Windows.Forms.IWin32Window"/> that will own the modal dialog box.</param>
        /// <param name="message">The text to display in the message box.  This text can be copied.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="icon">One of the <see cref="CopyableMessageBoxIcon"/> values that specifies which icon to display in the message box.</param>
        /// <param name="buttons">A <see cref="IList{T}"/> (where <c>T</c> is <see cref="string"/>) to display as buttons in the message box.</param>
        /// <param name="defBtn">The zero-based index of the default button.</param>
        /// <param name="cncBtn">The zero-based index of the cancel button.</param>
        /// <returns>The zero-based index of the button pressed.</returns>
        public static int Show(IWin32Window owner, string message, string caption, CopyableMessageBoxIcon icon, IList<string> buttons, int defBtn, int cncBtn)
        {
            CopyableMessageBoxInternal cmb = new CopyableMessageBoxInternal(message, caption, icon, buttons, defBtn, cncBtn);

            DialogResult dr;
            if (owner != null) { cmb.Icon = ((Form)owner).Icon; dr = cmb.ShowDialog(owner); } else { dr = cmb.ShowDialog(); }

            if (dr == DialogResult.Cancel) return cncBtn;
            return (cmb.theButton != null) ? buttons.IndexOf(cmb.theButton.Text) : -1;
        }

        private static int enumToCncBtn(CopyableMessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case CopyableMessageBoxButtons.OK: return 0;
                case CopyableMessageBoxButtons.OKCancel: return 1;
                case CopyableMessageBoxButtons.RetryCancel: return 1;
                case CopyableMessageBoxButtons.AbortRetryIgnore: return -1;
                case CopyableMessageBoxButtons.YesNoCancel: return 2;
                case CopyableMessageBoxButtons.YesNo: return -1;
                default: return -1;
            }
        }

        private static IList<string> enumToList(CopyableMessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case CopyableMessageBoxButtons.OKCancel: return new List<string>(new string[] { "&OK", "&Cancel", });
                case CopyableMessageBoxButtons.AbortRetryIgnore: return new List<string>(new string[] { "&Abort", "&Retry", "&Ignore", });
                case CopyableMessageBoxButtons.RetryCancel: return new List<string>(new string[] { "&Retry", "&Cancel", });
                case CopyableMessageBoxButtons.YesNoCancel: return new List<string>(new string[] { "&Yes", "&No", "&Cancel", });
                case CopyableMessageBoxButtons.YesNo: return new List<string>(new string[] { "&Yes", "&No", });
                case CopyableMessageBoxButtons.OK:
                default: return new List<string>(new string[] { "&OK", });
            }
        }

        /// <summary>
        /// Displays a message box containing the specified <see cref="Exception"/>
        /// including a full traceback of inner exceptions.
        /// The text can be copied.
        /// </summary>
        /// <param name="ex">A <see cref="Exception"/> to display.</param>
        public static void IssueException(Exception ex) { IssueException(ex, "", "Program Exception"); }
        /// <summary>
        /// Displays a message box containing the specified <see cref="Exception"/>
        /// including a full traceback of inner exceptions, with the specified caption.
        /// The text can be copied.
        /// </summary>
        /// <param name="ex">A <see cref="Exception"/> to display.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        public static void IssueException(Exception ex, string caption) { IssueException(ex, "", caption); }
        /// <summary>
        /// Displays a message box containing the specified <see cref="Exception"/>
        /// including a full traceback of inner exceptions, with the specified caption.
        /// The <paramref name="prefix"/> text is display before the exception trace.
        /// The text can be copied.
        /// </summary>
        /// <param name="ex">A <see cref="Exception"/> to display.</param>
        /// <param name="prefix">Text to display before the exception.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        public static void IssueException(Exception ex, string prefix, string caption)
        {
            System.Text.StringBuilder sb = new Text.StringBuilder();
            sb.Append(prefix);
            for (Exception inex = ex; inex != null; inex = inex.InnerException)
            {
                sb.Append("\nSource: " + inex.Source);
                sb.Append("\nAssembly: " + inex.TargetSite.DeclaringType.Assembly.FullName);
                sb.Append("\n" + inex.Message);
                sb.Append("\n" + inex.StackTrace);
                sb.Append("\n-----");
            }
            CopyableMessageBox.Show(sb.ToString(), caption, CopyableMessageBoxButtons.OK, CopyableMessageBoxIcon.Stop);
        }

    }
}
