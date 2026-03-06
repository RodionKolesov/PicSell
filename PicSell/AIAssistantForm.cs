using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using PluginBase;

namespace PicSell
{
    public partial class AIAssistantForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wp, string lp);
        private const uint EM_SETCUEBANNER = 0x1501;

        // ─── Провайдеры ───
        public enum AiProvider { Groq, Gemini, Ollama, Claude }

        private List<int> _imageIds;
        private string _apiKey = "";
        private AiProvider _provider = AiProvider.Groq;
        private string _ollamaModel = "llama3.2";
        private string _ollamaUrl = "http://localhost:11434";
        private bool _processing = false;
        private readonly List<(string role, string content)> _history = new List<(string, string)>();

        private const string SYSTEM_PROMPT =
            "Ты AI-ассистент для редактирования фотографий в приложении PicSell.\n" +
            "Пользователь может попросить отредактировать одну или несколько фотографий на русском или английском.\n" +
            "Преобразуй запрос пользователя в JSON с набором операций редактирования.\n\n" +
            "ДОСТУПНЫЕ ОПЕРАЦИИ:\n" +
            "{\"type\":\"brightness\",\"value\":N} — яркость, N от -100 до 100\n" +
            "{\"type\":\"contrast\",\"value\":N} — контрастность, N от -100 до 100\n" +
            "{\"type\":\"saturation\",\"value\":N} — насыщенность, N от -100 до 100 (−100 = ч/б)\n" +
            "{\"type\":\"hue\",\"value\":N} — сдвиг оттенка, N от -180 до 180 градусов\n" +
            "{\"type\":\"grayscale\"} — перевод в чёрно-белый\n" +
            "{\"type\":\"sepia\"} — эффект сепии\n" +
            "{\"type\":\"invert\"} — инверсия цветов\n" +
            "{\"type\":\"rotate\",\"angle\":N} — поворот, N = 90, 180 или 270\n" +
            "{\"type\":\"flip_horizontal\"} — отразить горизонтально\n" +
            "{\"type\":\"flip_vertical\"} — отразить вертикально\n" +
            "{\"type\":\"sharpen\",\"value\":N} — резкость, N от 1 до 5\n" +
            "{\"type\":\"blur\",\"radius\":N} — размытие, N от 1 до 10\n" +
            "{\"type\":\"resize\",\"percent\":N} — изменить размер (50=вдвое меньше, 200=вдвое больше)\n" +
            "{\"type\":\"watermark\",\"text\":\"ТЕКСТ\",\"position\":\"bottom_right\",\"opacity\":0.5,\"fontSize\":24} — водяной знак\n" +
            "{\"type\":\"remove_background\"} — удаление фона (нейросеть)\n\n" +
            "ФОРМАТ ОТВЕТА — строго только JSON без лишнего текста:\n" +
            "{\"message\":\"Краткое описание действий\",\"operations\":[...]}";

        public AIAssistantForm(List<int> imageIds)
        {
            InitializeComponent();
            _imageIds = imageIds;
            DarkTheme.Apply(this);
            ApplyCustomTheme();
            LoadSettings();
            UpdatePhotoCountLabel();
            UpdateProviderLabel();

            SendMessage(inputTextBox.Handle, EM_SETCUEBANNER, (IntPtr)1,
                "Напиши запрос… (Enter — отправить)");

            inputTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    _ = SendMessageAsync();
                }
            };
        }

        private void ApplyCustomTheme()
        {
            var inputBg = Color.FromArgb(32, 32, 32);
            var panelBg = Color.FromArgb(28, 28, 28);

            // Chat area
            chatRichTextBox.BackColor = Color.FromArgb(22, 22, 22);
            chatRichTextBox.ForeColor = DarkTheme.Text;
            chatRichTextBox.BorderStyle = BorderStyle.None;

            // Input panel background
            inputPanel.BackColor = panelBg;
            inputTableLayout.BackColor = inputBg;

            // Input textbox
            inputTextBox.BackColor = inputBg;
            inputTextBox.ForeColor = DarkTheme.Text;
            inputTextBox.BorderStyle = BorderStyle.None;

            // Info label
            titleLabel.BackColor = panelBg;
            titleLabel.ForeColor = DarkTheme.DimText;

            // Separator
            separatorPanel.BackColor = Color.FromArgb(50, 50, 50);

            // Send button — синий акцент
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.BackColor = DarkTheme.Accent;
            sendButton.ForeColor = Color.White;
            sendButton.UseVisualStyleBackColor = false;
            sendButton.FlatAppearance.BorderSize = 0;
            sendButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 140, 245);
            sendButton.Cursor = Cursors.Hand;

            // Settings button — прозрачный
            apiKeyButton.FlatStyle = FlatStyle.Flat;
            apiKeyButton.BackColor = inputBg;
            apiKeyButton.ForeColor = DarkTheme.DimText;
            apiKeyButton.FlatAppearance.BorderSize = 0;
            apiKeyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 45, 45);
            apiKeyButton.Cursor = Cursors.Hand;
        }

        private void UpdateInfoLabel()
        {
            string prov = _provider == AiProvider.Groq   ? "Groq"
                        : _provider == AiProvider.Gemini ? "Gemini"
                        : _provider == AiProvider.Claude ? "Claude"
                        : $"Ollama · {_ollamaModel}";
            string photos = _imageIds.Count == 0 ? "нет выбранных фото"
                          : $"{_imageIds.Count} фото";
            titleLabel.Text = $"{prov}  ·  {photos}";
            if (providerButton != null)
                providerButton.Text = prov + " ▾";
        }

        private void providerButton_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Groq").Click          += (s2, e2) => SetProvider(AiProvider.Groq);
            menu.Items.Add("Google Gemini").Click += (s2, e2) => SetProvider(AiProvider.Gemini);
            menu.Items.Add("Ollama").Click        += (s2, e2) => SetProvider(AiProvider.Ollama);
            menu.Items.Add("Claude").Click        += (s2, e2) => SetProvider(AiProvider.Claude);
            var btn = (Button)sender;
            menu.Show(btn, new System.Drawing.Point(0, btn.Height));
        }

        private void SetProvider(AiProvider p)
        {
            _provider = p;
            SaveSettings();
            UpdateInfoLabel();
            AppendMessage("Система", $"Провайдер переключён на: {_provider}", DarkTheme.DimText);
        }

        private void UpdatePhotoCountLabel() => UpdateInfoLabel();

        public void UpdateImageIds(List<int> ids)
        {
            _imageIds = ids;
            UpdateInfoLabel();
        }

        private void UpdateProviderLabel() => UpdateInfoLabel();

        // ─────────────────────────── Настройки ───────────────────────────

        private void LoadSettings()
        {
            try
            {
                var parser = new FileIniDataParser();
                if (!File.Exists("config.ini")) return;
                IniData data = parser.ReadFile("config.ini");
                if (!data.Sections.ContainsSection("AIAssistant")) return;

                string prov = data["AIAssistant"]["provider"] ?? "groq";
                if (prov == "gemini") _provider = AiProvider.Gemini;
                else if (prov == "ollama") _provider = AiProvider.Ollama;
                else if (prov == "claude") _provider = AiProvider.Claude;
                else _provider = AiProvider.Groq;

                _apiKey = data["AIAssistant"]["api_key"] ?? "";
                _ollamaModel = data["AIAssistant"]["ollama_model"] ?? "llama3.2";
                _ollamaUrl = data["AIAssistant"]["ollama_url"] ?? "http://localhost:11434";
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                var parser = new FileIniDataParser();
                IniData data = File.Exists("config.ini") ? parser.ReadFile("config.ini") : new IniData();
                if (!data.Sections.ContainsSection("AIAssistant"))
                    data.Sections.AddSection("AIAssistant");

                data["AIAssistant"]["provider"] = _provider.ToString().ToLower();
                data["AIAssistant"]["api_key"] = _apiKey;
                data["AIAssistant"]["ollama_model"] = _ollamaModel;
                data["AIAssistant"]["ollama_url"] = _ollamaUrl;
                parser.WriteFile("config.ini", data);
            }
            catch { }
        }

        private void apiKeyButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Настройки AI";
                dlg.Size = new Size(520, 310);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = DarkTheme.MainBg;

                int y = 14;

                // Провайдер
                var lblProv = MakeLabel("Провайдер:", 15, y);
                var cbProv = new ComboBox
                {
                    Location = new Point(15, y + 20),
                    Width = 480,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = DarkTheme.InputBg,
                    ForeColor = DarkTheme.Text,
                    FlatStyle = FlatStyle.Flat
                };
                cbProv.Items.AddRange(new object[]
                {
                    "Groq — бесплатно, регистрация на console.groq.com",
                    "Google Gemini — бесплатно, ключ на aistudio.google.com",
                    "Ollama — локально, без интернета (нужен ollama.ai)",
                    "Claude (Anthropic) — ключ на console.anthropic.com"
                });
                cbProv.SelectedIndex = (int)_provider;
                y += 54;

                // API ключ
                var lblKey = MakeLabel("API ключ (не нужен для Ollama):", 15, y);
                var tbKey = new TextBox
                {
                    Location = new Point(15, y + 20),
                    Width = 480,
                    Text = _apiKey,
                    BackColor = DarkTheme.InputBg,
                    ForeColor = DarkTheme.Text,
                    BorderStyle = BorderStyle.FixedSingle
                };
                y += 54;

                // Ollama
                var lblModel = MakeLabel("Ollama модель:", 15, y);
                var tbModel = new TextBox
                {
                    Location = new Point(15, y + 20),
                    Width = 200,
                    Text = _ollamaModel,
                    BackColor = DarkTheme.InputBg,
                    ForeColor = DarkTheme.Text,
                    BorderStyle = BorderStyle.FixedSingle
                };
                var lblUrl = MakeLabel("URL:", 225, y);
                var tbUrl = new TextBox
                {
                    Location = new Point(255, y + 20),
                    Width = 240,
                    Text = _ollamaUrl,
                    BackColor = DarkTheme.InputBg,
                    ForeColor = DarkTheme.Text,
                    BorderStyle = BorderStyle.FixedSingle
                };
                y += 54;

                // Подсказка
                var lblHint = MakeLabel("", 15, y);
                lblHint.ForeColor = DarkTheme.DimText;
                lblHint.Font = new Font("Segoe UI", 8.5f);
                cbProv.SelectedIndexChanged += (cs, ce) =>
                {
                    switch (cbProv.SelectedIndex)
                    {
                        case 0: lblHint.Text = "Groq: зарегистрируйся на console.groq.com → API Keys → Create Free Key"; break;
                        case 1: lblHint.Text = "Gemini: зайди на aistudio.google.com → Get API Key (бесплатно, нужен Google аккаунт)"; break;
                        case 2: lblHint.Text = "Ollama: установи с ollama.ai, затем запусти: ollama pull llama3.2"; break;
                        case 3: lblHint.Text = "Claude: зайди на console.anthropic.com → API Keys → Create Key"; break;
                    }
                };
                cbProv.SelectedIndex = (int)_provider; // trigger hint

                // Кнопка
                var btnSave = new Button { Text = "Сохранить", Location = new Point(15, y + 10), Width = 130 };
                DarkTheme.StyleButton(btnSave);
                btnSave.Click += (bs, be) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                dlg.Controls.AddRange(new Control[]
                {
                    lblProv, cbProv, lblKey, tbKey,
                    lblModel, tbModel, lblUrl, tbUrl,
                    lblHint, btnSave
                });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _provider = (AiProvider)cbProv.SelectedIndex;
                    _apiKey = tbKey.Text.Trim();
                    _ollamaModel = tbModel.Text.Trim().Length > 0 ? tbModel.Text.Trim() : "llama3.2";
                    _ollamaUrl = tbUrl.Text.Trim().Length > 0 ? tbUrl.Text.Trim() : "http://localhost:11434";
                    SaveSettings();
                    UpdateProviderLabel();
                    AppendMessage("Система", $"Настройки сохранены. Провайдер: {_provider}", DarkTheme.DimText);
                }
            }
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = DarkTheme.Text,
                BackColor = Color.Transparent
            };
        }

        // ─────────────────────────── Отправка сообщения ───────────────────────────

        private async void sendButton_Click(object sender, EventArgs e) => await SendMessageAsync();

        private async Task SendMessageAsync()
        {
            if (_processing) return;
            string text = inputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // Проверка настроек
            if (_provider != AiProvider.Ollama && string.IsNullOrEmpty(_apiKey))
            {
                string hint = _provider == AiProvider.Groq   ? "Бесплатный ключ: console.groq.com → API Keys"
                            : _provider == AiProvider.Claude ? "Ключ: console.anthropic.com → API Keys"
                            :                                  "Бесплатный ключ: aistudio.google.com → Get API Key";
                AppendMessage("Система",
                    "Сначала введите API ключ (кнопка «Настройки»).\n" + hint,
                    Color.OrangeRed);
                return;
            }

            inputTextBox.Clear();
            AppendMessage("Вы", text, Color.FromArgb(100, 180, 255));
            _processing = true;
            sendButton.Enabled = false;
            inputTextBox.Enabled = false;
            AppendMessage("Ассистент", "Обрабатываю запрос…", DarkTheme.DimText);

            try
            {
                string userMsg = text;
                string aiText = await Task.Run(() => CallAiApi(userMsg));

                ParsedResponse parsed = ParseAiResponse(aiText);
                if (parsed == null)
                {
                    AppendMessage("Ошибка", "Не удалось разобрать ответ:\n" + aiText, Color.OrangeRed);
                    return;
                }

                _history.Add(("user", userMsg));
                _history.Add(("assistant", aiText));

                AppendMessage("Ассистент", parsed.Message, DarkTheme.Accent);

                if (parsed.Operations == null || parsed.Operations.Count == 0)
                {
                    AppendMessage("Система", "Нет операций для выполнения.", DarkTheme.DimText);
                    return;
                }

                AppendMessage("Система",
                    $"Применяю {parsed.Operations.Count} операций к {_imageIds.Count} фото…",
                    DarkTheme.DimText);

                var ops = parsed.Operations;
                int done = 0;

                foreach (int imgId in _imageIds)
                {
                    try
                    {
                        Image result = await Task.Run<Image>(() =>
                        {
                            Image src = MainForm.Instance.LoadImageFromDB(imgId);
                            if (src == null) return null;
                            Image res = ApplyOperations(src, ops);
                            src.Dispose();
                            return res;
                        });

                        if (result != null)
                        {
                            MainForm.Instance.commitNewVersion(result, imgId, "AI: " + parsed.Message, "batch");
                            result.Dispose();
                            done++;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendMessage("Предупреждение", $"Ошибка фото {imgId}: {ex.Message}", Color.Orange);
                    }
                }

                MainForm.Instance.updateListView();
                AppendMessage("Система",
                    $"✓ Готово! Обработано {done} из {_imageIds.Count} фото.",
                    Color.FromArgb(80, 200, 80));
            }
            catch (Exception ex)
            {
                AppendMessage("Ошибка", ex.Message, Color.OrangeRed);
            }
            finally
            {
                _processing = false;
                sendButton.Enabled = true;
                inputTextBox.Enabled = true;
                inputTextBox.Focus();
            }
        }

        // ─────────────────────────── AI API вызовы ───────────────────────────

        private string CallAiApi(string userMessage)
        {
            switch (_provider)
            {
                case AiProvider.Groq:   return CallGroq(userMessage);
                case AiProvider.Gemini: return CallGemini(userMessage);
                case AiProvider.Ollama: return CallOllama(userMessage);
                case AiProvider.Claude: return CallClaude(userMessage);
                default:                return CallGroq(userMessage);
            }
        }

        // ── Groq (бесплатно, OpenAI-совместимый формат) ──
        private string CallGroq(string userMessage)
        {
            // Строим messages: history + новое сообщение
            var msgsSb = new StringBuilder();
            foreach (var (role, content) in _history)
                msgsSb.Append($"{{\"role\":\"{role}\",\"content\":\"{JsonEscape(content)}\"}},");
            msgsSb.Append($"{{\"role\":\"user\",\"content\":\"{JsonEscape(userMessage)}\"}}");

            string body =
                "{\"model\":\"llama-3.3-70b-versatile\"," +
                "\"max_tokens\":1024," +
                $"\"messages\":[{{\"role\":\"system\",\"content\":\"{JsonEscape(SYSTEM_PROMPT)}\"}},{msgsSb}]}}";

            return PostJson("https://api.groq.com/openai/v1/chat/completions",
                body,
                ("Authorization", "Bearer " + _apiKey));
        }

        // ── Google Gemini (бесплатный tier) ──
        private string CallGemini(string userMessage)
        {
            // Строим contents: history + новое сообщение
            var contentsSb = new StringBuilder();
            foreach (var (role, content) in _history)
            {
                string geminiRole = role == "assistant" ? "model" : "user";
                contentsSb.Append($"{{\"role\":\"{geminiRole}\",\"parts\":[{{\"text\":\"{JsonEscape(content)}\"}}]}},");
            }
            contentsSb.Append($"{{\"role\":\"user\",\"parts\":[{{\"text\":\"{JsonEscape(userMessage)}\"}}]}}");

            string body =
                "{\"systemInstruction\":{\"parts\":[{\"text\":\"" + JsonEscape(SYSTEM_PROMPT) + "\"}]}," +
                "\"generationConfig\":{\"maxOutputTokens\":1024}," +
                $"\"contents\":[{contentsSb}]}}";

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            return PostJson(url, body);
        }

        // ── Claude (Anthropic) ──
        private string CallClaude(string userMessage)
        {
            var msgsSb = new StringBuilder();
            bool first = true;
            foreach (var (role, content) in _history)
            {
                if (!first) msgsSb.Append(",");
                msgsSb.Append($"{{\"role\":\"{role}\",\"content\":\"{JsonEscape(content)}\"}}");
                first = false;
            }
            if (!first) msgsSb.Append(",");
            msgsSb.Append($"{{\"role\":\"user\",\"content\":\"{JsonEscape(userMessage)}\"}}");

            string body =
                "{\"model\":\"claude-sonnet-4-6\"," +
                "\"max_tokens\":1024," +
                $"\"system\":\"{JsonEscape(SYSTEM_PROMPT)}\"," +
                $"\"messages\":[{msgsSb}]}}";

            return PostJson("https://api.anthropic.com/v1/messages",
                body,
                ("x-api-key", _apiKey),
                ("anthropic-version", "2023-06-01"));
        }

        // ── Ollama (локально, полностью бесплатно) ──
        private string CallOllama(string userMessage)
        {
            var msgsSb = new StringBuilder();
            msgsSb.Append($"{{\"role\":\"system\",\"content\":\"{JsonEscape(SYSTEM_PROMPT)}\"}},");
            foreach (var (role, content) in _history)
                msgsSb.Append($"{{\"role\":\"{role}\",\"content\":\"{JsonEscape(content)}\"}},");
            msgsSb.Append($"{{\"role\":\"user\",\"content\":\"{JsonEscape(userMessage)}\"}}");

            string body =
                $"{{\"model\":\"{_ollamaModel}\"," +
                "\"stream\":false," +
                $"\"messages\":[{msgsSb}]}}";

            return PostJson(_ollamaUrl.TrimEnd('/') + "/api/chat", body);
        }

        private string PostJson(string url, string body, params (string name, string value)[] headers)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                foreach (var (name, value) in headers)
                    client.DefaultRequestHeaders.Add(name, value);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        // ─────────────────────────── Парсинг ответа ───────────────────────────

        private string ExtractAiText(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new Exception("Пустой ответ от API");

            // Groq / OpenAI: {"choices":[{"message":{"content":"..."}}]}
            if (json.Contains("\"choices\""))
            {
                int idx = json.IndexOf("\"content\":", json.IndexOf("\"choices\""));
                if (idx >= 0) return ExtractJsonStringAt(json, idx + 10);
            }

            // Gemini: {"candidates":[{"content":{"parts":[{"text":"..."}]}}]}
            if (json.Contains("\"candidates\""))
            {
                int idx = json.IndexOf("\"text\":", json.IndexOf("\"candidates\""));
                if (idx >= 0) return ExtractJsonStringAt(json, idx + 7);
            }

            // Claude: {"content":[{"type":"text","text":"..."}]}
            if (json.Contains("\"type\":\"text\"") || (json.Contains("\"content\"") && json.Contains("\"text\"")))
            {
                int idx = json.IndexOf("\"text\":", json.IndexOf("\"content\""));
                if (idx >= 0) return ExtractJsonStringAt(json, idx + 7);
            }

            // Ollama: {"message":{"content":"..."}}
            if (json.Contains("\"message\"") && json.Contains("\"content\""))
            {
                int msgIdx = json.IndexOf("\"message\"");
                int idx = json.IndexOf("\"content\":", msgIdx);
                if (idx >= 0) return ExtractJsonStringAt(json, idx + 10);
            }

            // Ошибка API
            int errIdx = json.IndexOf("\"error\"");
            if (errIdx >= 0)
            {
                int msgIdx2 = json.IndexOf("\"message\":", errIdx);
                string errMsg = msgIdx2 >= 0 ? ExtractJsonStringAt(json, msgIdx2 + 10) : json.Substring(0, Math.Min(200, json.Length));
                throw new Exception("Ошибка API: " + errMsg);
            }

            throw new Exception("Неизвестный формат ответа: " + json.Substring(0, Math.Min(200, json.Length)));
        }

        private string ExtractJsonStringAt(string json, int pos)
        {
            int i = pos;
            while (i < json.Length && json[i] != '"') i++;
            if (i >= json.Length) return null;
            i++;
            var sb = new StringBuilder();
            while (i < json.Length)
            {
                char c = json[i];
                if (c == '\\' && i + 1 < json.Length)
                {
                    i++;
                    switch (json[i])
                    {
                        case '"':  sb.Append('"');  break;
                        case '\\': sb.Append('\\'); break;
                        case '/':  sb.Append('/');  break;
                        case 'n':  sb.Append('\n'); break;
                        case 'r':  sb.Append('\r'); break;
                        case 't':  sb.Append('\t'); break;
                        default:   sb.Append('\\'); sb.Append(json[i]); break;
                    }
                }
                else if (c == '"') break;
                else sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        private ParsedResponse ParseAiResponse(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return null;
            try
            {
                // Сначала извлекаем реальный текст из ответа API
                string text = ExtractAiText(rawText);

                // Убираем markdown блоки
                if (text.Contains("```"))
                {
                    int blockStart = text.IndexOf("```");
                    int jsonStart = text.IndexOf('{', blockStart);
                    int blockEnd = text.LastIndexOf("```");
                    if (jsonStart >= 0 && blockEnd > jsonStart)
                        text = text.Substring(jsonStart, blockEnd - jsonStart).Trim();
                }

                // Извлекаем JSON объект
                int firstBrace = text.IndexOf('{');
                int lastBrace = text.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                    text = text.Substring(firstBrace, lastBrace - firstBrace + 1);

                string message = "";
                int msgIdx = text.IndexOf("\"message\":");
                if (msgIdx >= 0) message = ExtractJsonStringAt(text, msgIdx + 10) ?? "";

                var operations = new List<Dictionary<string, object>>();
                int opsIdx = text.IndexOf("\"operations\":");
                if (opsIdx >= 0)
                {
                    int arrStart = text.IndexOf('[', opsIdx);
                    if (arrStart >= 0) operations = ParseJsonArray(text, arrStart);
                }

                return new ParsedResponse { Message = message, Operations = operations };
            }
            catch (Exception ex)
            {
                AppendMessage("Ошибка", ex.Message, Color.OrangeRed);
                return null;
            }
        }

        private List<Dictionary<string, object>> ParseJsonArray(string json, int arrStart)
        {
            var result = new List<Dictionary<string, object>>();
            int i = arrStart + 1;
            while (i < json.Length)
            {
                while (i < json.Length && (json[i] == ' ' || json[i] == '\n' || json[i] == '\r' || json[i] == '\t' || json[i] == ',')) i++;
                if (i >= json.Length || json[i] == ']') break;
                if (json[i] == '{')
                {
                    int objStart = i, brace = 0;
                    do { if (json[i] == '{') brace++; else if (json[i] == '}') brace--; i++; }
                    while (brace > 0 && i < json.Length);
                    result.Add(ParseJsonObject(json.Substring(objStart, i - objStart)));
                }
                else i++;
            }
            return result;
        }

        private Dictionary<string, object> ParseJsonObject(string json)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            int i = 0;
            while (i < json.Length)
            {
                while (i < json.Length && json[i] != '"') i++;
                if (i >= json.Length) break;
                i++;
                var key = new StringBuilder();
                while (i < json.Length && json[i] != '"') { key.Append(json[i]); i++; }
                i++;
                while (i < json.Length && json[i] != ':') i++;
                i++;
                while (i < json.Length && (json[i] == ' ' || json[i] == '\n' || json[i] == '\r')) i++;
                if (i >= json.Length) break;

                object value;
                if (json[i] == '"')
                {
                    i++;
                    var val = new StringBuilder();
                    while (i < json.Length && json[i] != '"')
                    {
                        if (json[i] == '\\' && i + 1 < json.Length) { i++; switch (json[i]) { case '"': val.Append('"'); break; case '\\': val.Append('\\'); break; case 'n': val.Append('\n'); break; default: val.Append(json[i]); break; } }
                        else val.Append(json[i]);
                        i++;
                    }
                    i++;
                    value = val.ToString();
                }
                else if (json[i] == '-' || char.IsDigit(json[i]))
                {
                    var num = new StringBuilder();
                    while (i < json.Length && (json[i] == '-' || json[i] == '.' || char.IsDigit(json[i]))) { num.Append(json[i]); i++; }
                    string ns = num.ToString();
                    value = ns.Contains('.') ? (object)double.Parse(ns, System.Globalization.CultureInfo.InvariantCulture) : long.Parse(ns);
                }
                else if (i + 4 <= json.Length && json.Substring(i, 4) == "true")  { value = true;  i += 4; }
                else if (i + 5 <= json.Length && json.Substring(i, 5) == "false") { value = false; i += 5; }
                else { i++; continue; }

                dict[key.ToString()] = value;
            }
            return dict;
        }

        // ─────────────────────────── Применение операций ───────────────────────────

        private Image ApplyOperations(Image source, List<Dictionary<string, object>> operations)
        {
            Bitmap bmp = new Bitmap(source);
            foreach (var op in operations)
            {
                if (!op.ContainsKey("type")) continue;
                string type = op["type"]?.ToString();
                try
                {
                    switch (type)
                    {
                        case "brightness":    bmp = ApplyColorMatrix(bmp, MakeBrightnessMatrix(GetInt(op, "value", 0))); break;
                        case "contrast":      bmp = ApplyColorMatrix(bmp, MakeContrastMatrix(GetInt(op, "value", 0))); break;
                        case "saturation":    bmp = ApplyColorMatrix(bmp, MakeSaturationMatrix(GetInt(op, "value", 0))); break;
                        case "hue":           bmp = ApplyHue(bmp, GetInt(op, "value", 0)); break;
                        case "grayscale":     bmp = ApplyColorMatrix(bmp, MakeGrayscaleMatrix()); break;
                        case "sepia":         bmp = ApplyColorMatrix(bmp, MakeSepiaMatrix()); break;
                        case "invert":        bmp = ApplyColorMatrix(bmp, MakeInvertMatrix()); break;
                        case "rotate":        bmp = ApplyRotate(bmp, GetInt(op, "angle", 90)); break;
                        case "flip_horizontal": bmp.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                        case "flip_vertical":   bmp.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
                        case "sharpen":       bmp = ApplySharpen(bmp, GetInt(op, "value", 2)); break;
                        case "blur":          bmp = ApplyBoxBlur(bmp, GetInt(op, "radius", 3)); break;
                        case "resize":        bmp = ApplyResize(bmp, GetInt(op, "percent", 100)); break;
                        case "watermark":
                            bmp = ApplyWatermark(bmp,
                                op.ContainsKey("text") ? op["text"]?.ToString() : "PicSell",
                                op.ContainsKey("position") ? op["position"]?.ToString() : "bottom_right",
                                GetFloat(op, "opacity", 0.5f),
                                GetInt(op, "fontSize", 24));
                            break;
                        case "remove_background": bmp = ApplyRemoveBackground(bmp); break;
                    }
                }
                catch { }
            }
            return bmp;
        }

        private Bitmap ApplyRemoveBackground(Bitmap src)
        {
            try
            {
                var plugin = MainForm.Instance.GetRemoveBackPlugin();
                Image removed = plugin.ProcessImage(src);
                src.Dispose();
                var result = new Bitmap(removed);
                removed.Dispose();
                return result;
            }
            catch { return src; }
        }

        // ─────────────────────────── ColorMatrix ───────────────────────────

        private Bitmap ApplyColorMatrix(Bitmap src, ColorMatrix cm)
        {
            var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            using (var attrs = new ImageAttributes())
            {
                attrs.SetColorMatrix(cm);
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, attrs);
            }
            src.Dispose();
            return result;
        }

        private ColorMatrix MakeBrightnessMatrix(int v) { float d = v / 255f; return new ColorMatrix(new[] { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { d, d, d, 0, 1 } }); }
        private ColorMatrix MakeContrastMatrix(int v) { float c = (100f + v) / 100f, t = (1f - c) / 2f; return new ColorMatrix(new[] { new float[] { c, 0, 0, 0, 0 }, new float[] { 0, c, 0, 0, 0 }, new float[] { 0, 0, c, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { t, t, t, 0, 1 } }); }
        private ColorMatrix MakeSaturationMatrix(int v) { float s = (100f + v) / 100f, r = 0.3086f * (1 - s), g = 0.6094f * (1 - s), b = 0.0820f * (1 - s); return new ColorMatrix(new[] { new float[] { r + s, r, r, 0, 0 }, new float[] { g, g + s, g, 0, 0 }, new float[] { b, b, b + s, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } }); }
        private ColorMatrix MakeGrayscaleMatrix() { float r = 0.3086f, g = 0.6094f, b = 0.0820f; return new ColorMatrix(new[] { new float[] { r, r, r, 0, 0 }, new float[] { g, g, g, 0, 0 }, new float[] { b, b, b, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } }); }
        private ColorMatrix MakeSepiaMatrix() { return new ColorMatrix(new[] { new float[] { 0.393f, 0.349f, 0.272f, 0, 0 }, new float[] { 0.769f, 0.686f, 0.534f, 0, 0 }, new float[] { 0.189f, 0.168f, 0.131f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } }); }
        private ColorMatrix MakeInvertMatrix() { return new ColorMatrix(new[] { new float[] { -1, 0, 0, 0, 0 }, new float[] { 0, -1, 0, 0, 0 }, new float[] { 0, 0, -1, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 1, 1, 1, 0, 1 } }); }

        // ─────────────────────────── Пиксельные операции ───────────────────────────

        private Bitmap ApplyHue(Bitmap src, int delta)
        {
            if (delta == 0) return src;
            var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    RgbToHsl(c.R, c.G, c.B, out float h, out float s, out float l);
                    h = ((h + delta) % 360f + 360f) % 360f;
                    HslToRgb(h, s, l, out byte nr, out byte ng, out byte nb);
                    result.SetPixel(x, y, Color.FromArgb(c.A, nr, ng, nb));
                }
            src.Dispose(); return result;
        }

        private Bitmap ApplyBoxBlur(Bitmap src, int radius)
        {
            if (radius <= 0) return src;
            int r = Math.Min(radius, 10);
            var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    int tR = 0, tG = 0, tB = 0, tA = 0, n = 0;
                    for (int ky = -r; ky <= r; ky++)
                        for (int kx = -r; kx <= r; kx++)
                        {
                            Color c = src.GetPixel(Math.Max(0, Math.Min(src.Width - 1, x + kx)), Math.Max(0, Math.Min(src.Height - 1, y + ky)));
                            tR += c.R; tG += c.G; tB += c.B; tA += c.A; n++;
                        }
                    result.SetPixel(x, y, Color.FromArgb(tA / n, tR / n, tG / n, tB / n));
                }
            src.Dispose(); return result;
        }

        private Bitmap ApplySharpen(Bitmap src, int strength)
        {
            float s = strength * 0.5f;
            float[,] k = { { 0, -s / 2, 0 }, { -s / 2, 1 + 2 * s, -s / 2 }, { 0, -s / 2, 0 } };
            var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            for (int y = 1; y < src.Height - 1; y++)
                for (int x = 1; x < src.Width - 1; x++)
                {
                    float fr = 0, fg = 0, fb = 0;
                    for (int ky = -1; ky <= 1; ky++)
                        for (int kx = -1; kx <= 1; kx++) { Color c = src.GetPixel(x + kx, y + ky); fr += c.R * k[ky + 1, kx + 1]; fg += c.G * k[ky + 1, kx + 1]; fb += c.B * k[ky + 1, kx + 1]; }
                    result.SetPixel(x, y, Color.FromArgb(src.GetPixel(x, y).A, Clamp((int)fr), Clamp((int)fg), Clamp((int)fb)));
                }
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    if (y == 0 || y == src.Height - 1 || x == 0 || x == src.Width - 1)
                        result.SetPixel(x, y, src.GetPixel(x, y));
            src.Dispose(); return result;
        }

        private Bitmap ApplyRotate(Bitmap src, int angle)
        {
            RotateFlipType rft = angle == 90 ? RotateFlipType.Rotate90FlipNone : angle == 180 ? RotateFlipType.Rotate180FlipNone : angle == 270 ? RotateFlipType.Rotate270FlipNone : RotateFlipType.RotateNoneFlipNone;
            src.RotateFlip(rft); return src;
        }

        private Bitmap ApplyResize(Bitmap src, int percent)
        {
            if (percent <= 0 || percent == 100) return src;
            int nw = Math.Max(1, src.Width * percent / 100), nh = Math.Max(1, src.Height * percent / 100);
            var result = new Bitmap(nw, nh, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result)) { g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; g.DrawImage(src, 0, 0, nw, nh); }
            src.Dispose(); return result;
        }

        private Bitmap ApplyWatermark(Bitmap src, string text, string position, float opacity, int fontSize)
        {
            var result = new Bitmap(src);
            using (var g = Graphics.FromImage(result))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using (var font = new Font("Arial", Math.Max(8, Math.Min(fontSize, 200)), FontStyle.Bold))
                {
                    SizeF sz = g.MeasureString(text, font);
                    float pad = 20;
                    PointF pt = position == "top_left" ? new PointF(pad, pad) : position == "top_right" ? new PointF(src.Width - sz.Width - pad, pad) : position == "bottom_left" ? new PointF(pad, src.Height - sz.Height - pad) : position == "center" ? new PointF((src.Width - sz.Width) / 2f, (src.Height - sz.Height) / 2f) : new PointF(src.Width - sz.Width - pad, src.Height - sz.Height - pad);
                    int alpha = (int)(255 * Math.Max(0f, Math.Min(1f, opacity)));
                    using (var shadow = new SolidBrush(Color.FromArgb(alpha / 2, 0, 0, 0))) g.DrawString(text, font, shadow, pt.X + 2, pt.Y + 2);
                    using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.White))) g.DrawString(text, font, brush, pt);
                }
            }
            src.Dispose(); return result;
        }

        // ─────────────────────────── HSL ───────────────────────────

        private static void RgbToHsl(byte r, byte g, byte b, out float h, out float s, out float l)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
            l = (max + min) / 2f;
            if (max == min) { h = 0; s = 0; return; }
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            h = max == rf ? ((gf - bf) / d + (gf < bf ? 6 : 0)) * 60f : max == gf ? ((bf - rf) / d + 2f) * 60f : ((rf - gf) / d + 4f) * 60f;
        }

        private static void HslToRgb(float h, float s, float l, out byte r, out byte g, out byte b)
        {
            if (s == 0) { r = g = b = (byte)(l * 255); return; }
            float q = l < 0.5f ? l * (1 + s) : l + s - l * s, p = 2 * l - q;
            r = (byte)(HueToRgb(p, q, h / 360f + 1f / 3f) * 255);
            g = (byte)(HueToRgb(p, q, h / 360f) * 255);
            b = (byte)(HueToRgb(p, q, h / 360f - 1f / 3f) * 255);
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0) t += 1; if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 0.5f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        // ─────────────────────────── Утилиты ───────────────────────────

        private static int Clamp(int v) => Math.Max(0, Math.Min(255, v));
        private static int GetInt(Dictionary<string, object> d, string key, int def) { if (!d.ContainsKey(key)) return def; try { return Convert.ToInt32(d[key]); } catch { return def; } }
        private static float GetFloat(Dictionary<string, object> d, string key, float def) { if (!d.ContainsKey(key)) return def; try { return Convert.ToSingle(d[key]); } catch { return def; } }
        private static string JsonEscape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

        private void AppendMessage(string sender, string text, Color color)
        {
            if (chatRichTextBox.InvokeRequired) { chatRichTextBox.Invoke(new Action(() => AppendMessage(sender, text, color))); return; }
            chatRichTextBox.SelectionStart = chatRichTextBox.TextLength;
            chatRichTextBox.SelectionLength = 0;

            if (sender == "Вы")
            {
                // Сообщение пользователя — справа, жирный лейбл
                chatRichTextBox.SelectionColor = Color.FromArgb(70, 130, 220);
                chatRichTextBox.SelectionFont = new Font(chatRichTextBox.Font, FontStyle.Bold);
                chatRichTextBox.AppendText("\nВы  ");
                chatRichTextBox.SelectionFont = chatRichTextBox.Font;
                chatRichTextBox.SelectionColor = Color.FromArgb(200, 210, 230);
                chatRichTextBox.AppendText(text + "\n");
            }
            else if (sender == "Ассистент")
            {
                // Ответ AI — просто текст без лейбла
                chatRichTextBox.SelectionColor = color;
                chatRichTextBox.AppendText("\n" + text + "\n");
            }
            else if (sender == "Система")
            {
                // Системные — маленький серый текст
                chatRichTextBox.SelectionColor = Color.FromArgb(70, 70, 70);
                chatRichTextBox.AppendText("  " + text + "\n");
            }
            else
            {
                // Ошибки и прочее
                chatRichTextBox.SelectionColor = color;
                chatRichTextBox.AppendText($"\n{sender}: {text}\n");
            }

            chatRichTextBox.SelectionStart = chatRichTextBox.TextLength;
            chatRichTextBox.ScrollToCaret();
        }

        private class ParsedResponse
        {
            public string Message { get; set; }
            public List<Dictionary<string, object>> Operations { get; set; }
        }
    }
}
