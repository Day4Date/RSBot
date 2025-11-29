using System;
using System.Windows.Forms;
using Python.Runtime;
using RSBot.Python.Components.API.GUI.Wrapper;
using RSBot.Python.Views;

namespace RSBot.Python.Components.API.GUI.Controls
{
    public class TextBoxWrapper : GuiControlWrapper
    {
        private PyObject _callback;

        public TextBoxWrapper(TextBox tb, Main form, PyObject callback = null)
            : base(tb, form)
        {
            _callback = callback;

            if (_callback != null)
            {
                tb.TextChanged += (sender, args) =>
                {
                    using (Py.GIL())
                    {
                        try
                        {
                            _callback.Invoke(new PyString(tb.Text));
                        }
                        catch (PythonException ex)
                        {
                            form.AppendLog(ex.ToString());
                        }
                    }
                };
            }
        }

        public string GetText()
        {
            return ((TextBox)Control).Text;
        }

        public void SetTextValue(string text)
        {
            Invoke(() => ((TextBox)Control).Text = text);
        }
    }
}
