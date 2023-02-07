using RimGen.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RimGen
{
    public partial class Form1 : Form
    {
        public static string Lang = "";
        public static bool SaveScreen = true;
        public static bool FireAttr = true;

        private const string SETTINGS_FILE_NAME = "settings.config";

        public Form1()
        {
            InitializeComponent();
            Log("");

            RestoreSettings();

            if (Form1.Lang == "")
            {
                var culture = Thread.CurrentThread.CurrentUICulture;
                if (culture.ToString().ToLower().Contains("ru"))
                {
                    rbRu.Checked = true;
                }
                else
                {
                    rbEn.Checked = true;
                }
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ExStyle |= 8;  // Turn on WS_EX_TOPMOST
        //        return cp;
        //    }
        //}

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            var conditions = GetConditions();
            //сохраняем при генерации
            SaveSettings(conditions);

            if (conditions.Count > 0)
            {
                Log(Form1.Lang == "ru" ? "Генерим..." : "Generating...");
                var resultMsg = Generator.Generate(conditions, 60 * 5);
                Log(resultMsg);
            }
            else
            {
                Log(Form1.Lang == "ru" ? "Задайте любой аттрибут > 0" : "Set any attribute > 0");
            }
        }

        private void SaveSettings(List<Condition> conditions)
        {
            var result = "";

            //screen
            result += $"screen:{Form1.SaveScreen};";

            //fire
            result += $"fire:{Form1.FireAttr};";

            //lang
            result += $"lang:{Form1.Lang};";

            foreach (var cond in conditions)
            {
                result += $"{(int)cond.Attr}:{cond.MinValue};";
            }
            File.WriteAllText(SETTINGS_FILE_NAME, result);
        }

        private void RestoreSettings()
        {
            List<Condition> conditions = LoadSettings();

            if (conditions.Count > 0)
            {
                foreach (var cond in conditions)
                {
                    var ix = (int)cond.Attr + 1;
                    var tb = this.Controls.Find($"textBox{ix}", false)[0];
                    if (tb != null)
                    {
                        var value = ValidateValue(cond.MinValue.ToString());
                        tb.Text = value;
                    }
                }
            }

            if (Form1.Lang == "ru")
            {
                rbRu.Checked = true;
            }
            else if (Form1.Lang == "en")
            {
                rbEn.Checked = true;
            }

            chkbScreen.Checked = Form1.SaveScreen;
            chkbFire.Checked = Form1.FireAttr;
        }

        private List<Condition> LoadSettings()
        {
            List<Condition> conditions = new List<Condition>();

            try
            {
                var result = File.ReadAllText(SETTINGS_FILE_NAME);
                if (result != null && result.Length > 0)
                {
                    var condList = result.Split(';');
                    foreach (var cond in condList)
                    {
                        var prms = cond.Split(':');
                        if (prms.Length == 2)
                        {
                            if (prms[0] == "lang")
                            {
                                Form1.Lang = prms[1];
                            }
                            else if (prms[0] == "screen")
                            {
                                var saveScreen = false;
                                if (bool.TryParse(prms[1], out saveScreen))
                                {
                                    Form1.SaveScreen = saveScreen;
                                }
                            }
                            else if (prms[0] == "fire")
                            {
                                var fireAttr = false;
                                if (bool.TryParse(prms[1], out fireAttr))
                                {
                                    Form1.FireAttr = fireAttr;
                                }
                            }
                            else
                            {
                                var attr = 0;
                                var value = 0;

                                if (int.TryParse(prms[0], out attr) && int.TryParse(prms[1], out value))
                                {
                                    conditions.Add(new Condition((AttributeEnum)attr, value));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { };

            return conditions;
        }

        private List<Condition> GetConditions()
        {
            var list = new List<Condition>();

            for (var i = 1; i <= 12; i++)
            {
                var tb = this.Controls.Find($"textBox{i}", false)[0];

                var val = 0;
                int.TryParse(tb.Text, out val);

                if (val > 0)
                {
                    var attr = (AttributeEnum)(i - 1);
                    list.Add(new Condition(attr, val));
                }
            }

            return list;
        }

        private void Log(string text)
        {
            if (text.Contains("Ошибка!") || text.Contains("Error!"))
            {
                lblLog.ForeColor = Color.Red;
            }
            else
            {
                lblLog.ForeColor = Color.Black;
            }
            lblLog.Text = text;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                var value = tb.Text;
                value = ValidateValue(value);
                tb.Text = value;
            }
        }

        private void textBox_SelectAll(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private string ValidateValue(string value)
        {
            if (value.Length > 2)
            {
                value = value.Substring(value.Length - 1, 1) + value.Substring(0, 1);
                //value = value.Substring(0, 2);
            }

            var intValue = -1;
            if (!int.TryParse(value, out intValue))
            {
                value = "";
            }

            return value;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRu.Checked)
            {
                Form1.Lang = "ru";
            }
            else
            {
                Form1.Lang = "en";
            }
            SetLang();
        }

        private void SetLang()
        {
            for (var i = 1; i <= 12; i++)
            {
                var tb = this.Controls.Find($"label{i}", false)[0];
                var title = GetTitleByAttrEnum((AttributeEnum)(i - 1));
                tb.Text = title;
            }

            buttonGenerate.Text = Form1.Lang == "ru" ? "Сгенерировать" : "Generate";
            chkbScreen.Text = Form1.Lang == "ru" ? "Сохр. скрин когда нашел" : "Save screen when found";
            lblLog.Text = "";
        }

        private string GetTitleByAttrEnum(AttributeEnum attr)
        {
            switch (attr)
            {
                case AttributeEnum.Shooting: return Form1.Lang == "ru" ? "Дальний бой" : "Shooting";
                case AttributeEnum.Melee: return Form1.Lang == "ru" ? "Ближний бой" : "Melee";
                case AttributeEnum.Construction: return Form1.Lang == "ru" ? "Строительство" : "Construction";
                case AttributeEnum.Mining: return Form1.Lang == "ru" ? "Горное дело" : "Mining";
                case AttributeEnum.Cooking: return Form1.Lang == "ru" ? "Кулинария" : "Cooking";
                case AttributeEnum.Growing: return Form1.Lang == "ru" ? "Растениеводство" : "Plants";
                case AttributeEnum.Animals: return Form1.Lang == "ru" ? "Животноводство" : "Animals";
                case AttributeEnum.Crafting: return Form1.Lang == "ru" ? "Ремесло" : "Crafting";
                case AttributeEnum.Artistic: return Form1.Lang == "ru" ? "Искусство" : "Artistic";
                case AttributeEnum.Medicine: return Form1.Lang == "ru" ? "Медицина" : "Medical";
                case AttributeEnum.Social: return Form1.Lang == "ru" ? "Общение" : "Social";
                case AttributeEnum.Research: return Form1.Lang == "ru" ? "Умственный труд" : "Intellectual";
            }

            return "";
        }

        private void chkbScreen_CheckedChanged(object sender, EventArgs e)
        {
            Form1.SaveScreen = chkbScreen.Checked;
        }

        private void chkbFire_CheckedChanged(object sender, EventArgs e)
        {
            Form1.FireAttr = chkbFire.Checked;
        }
    }
}
