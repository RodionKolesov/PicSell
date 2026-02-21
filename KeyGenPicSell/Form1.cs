using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using static KeyGenPicSell.LicenseData;

namespace KeyGenPicSell
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void DisplayUsbSerial()
        {
            if (comboBoxUSB.Items.Count > 0)
            {
                string usbSerial = GetSelectedUsbSerial();
                if (!string.IsNullOrEmpty(usbSerial))
                {
                    labelSn.Text = usbSerial;
                }
                else
                {
                    labelSn.Text = "Не удалось получить серийный номер";
                }
            }
            else
            {
                labelSn.Text = "USB-устройство не найдено";
            }
        }

        private void RefreshUsbList()
        {
            comboBoxUSB.Items.Clear();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    if (drive.IsReady)
                    {
                        comboBoxUSB.Items.Add($"{drive.Name} ({drive.VolumeLabel})");
                    }
                    else
                    {
                        comboBoxUSB.Items.Add($"{drive.Name} (Не готов)");
                    }
                }
            }

            if (comboBoxUSB.Items.Count > 0)
                comboBoxUSB.SelectedIndex = 0;

            DisplayUsbSerial();
        }

        private string GetSelectedUsbSerial()
        {
            if (comboBoxUSB.SelectedIndex == -1) return null;

            string driveLetter = comboBoxUSB.SelectedItem.ToString().Substring(0, 2);
            var drive = new DriveInfo(driveLetter);

            string query = "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'";
            ManagementObjectSearcher searcher = null;
            ManagementObjectCollection collection = null;

            try
            {
                searcher = new ManagementObjectSearcher(query);
                collection = searcher.Get();

                foreach (ManagementObject diskDrive in collection)
                {
                    try
                    {
                        string DeviceIDstr = (string)diskDrive["PNPDeviceID"];
                        string Serial = Retrieve_serial(DeviceIDstr);
                        return Serial;
                    }
                    finally
                    {
                        if (diskDrive != null)
                            diskDrive.Dispose();
                    }
                }
            }
            finally
            {
                if (collection != null)
                    collection.Dispose();
                if (searcher != null)
                    searcher.Dispose();
            }

            return null;
        }

        public static string Retrieve_serial(string strSource)
        {
            string strStart = "\\";
            int Start, End;
            Start = strSource.LastIndexOf(strStart) + strStart.Length;
            End = strSource.IndexOf("&0", Start);
            string serial = strSource.Substring(Start, End - Start);
            return serial;
        }

        private void GenerateLicenseKey()
        {
            if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                MessageBox.Show("Введите имя пользователя");
                return;
            }

            string usbSerial = GetSelectedUsbSerial();
            if (string.IsNullOrEmpty(usbSerial))
            {
                MessageBox.Show("Не удалось получить серийный номер USB.");
                return;
            }

            var licenseData = new LicenseData
            {
                UserName = textBoxUsername.Text,
                UserProfile = comboBoxVersions.SelectedItem?.ToString(),
                disableStat = chkStat.Checked,
                disablePlugin = chkPlugins.Checked,
                disableDraw = chkDraw.Checked,
                ExpirationDate = dateTimePicker1.Value
            };

            if (comboBoxVersions.SelectedItem?.ToString() == "Full")
            {
                licenseData.disableStat = false;
                licenseData.disablePlugin = false;
                licenseData.disableDraw = false;
            }

            var keyGenerator = new LicenseKeyGenerator(); 
            byte[] licenseKey = keyGenerator.GenerateKey(licenseData, usbSerial);

            labelSn.Text = usbSerial;
            btnSaveKey.Enabled = true;
            Tag = licenseKey;
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            GenerateLicenseKey();
        }

        private void SaveLicenseKey()
        {
            if (Tag is byte[] licenseKey)
            {
                if (comboBoxUSB.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите USB-устройство.");
                    return;
                }

                string driveLetter = comboBoxUSB.SelectedItem.ToString().Substring(0, 2);
                string licensePath = Path.Combine(driveLetter, "license.lic");

                try
                {
                    File.WriteAllBytes(licensePath, licenseKey);
                    MessageBox.Show("Ключ успешно сохранен!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения: " + ex.Message);
                }
            }
        }

        private void btnSaveKey_Click(object sender, EventArgs e)
        {
            SaveLicenseKey();
        }

        private void comboBoxUSB_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUsbSerial();
        }

        private void btnRefreshUsb_Click(object sender, EventArgs e)
        {
            RefreshUsbList();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            RefreshUsbList();
            comboBoxVersions.Items.AddRange(new string[] { "Demo", "Full" });
            comboBoxVersions.SelectedIndex = 0;
        }

        private void comboBoxVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxVersions.SelectedIndex == 1)
            {
                limitationsGroupBox.Enabled = false;
            }
            if (comboBoxVersions.SelectedIndex == 0)
            {
                limitationsGroupBox.Enabled = true;
            }
        }
    }

    [Serializable]
    public class LicenseData
    {
        public string UserName { get; set; }             // Имя пользователя
        public string UserProfile { get; set; }          // Тип лицензии: "Demo" / "Full"
        public bool disableStat { get; set; }
        public bool disablePlugin { get; set; }
        public bool disableDraw { get; set; }
        public DateTime ExpirationDate { get; set; }     // Дата окончания лицензии


        [Serializable]
        public class LicenseContainer
        {
            public LicenseData License { get; set; }
            public string UsbSerial { get; set; }

            public byte[] Serialize()
            {
                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms, Encoding.UTF8))
                {
                    writer.Write(License.UserName ?? string.Empty);
                    writer.Write(License.UserProfile ?? string.Empty);
                    writer.Write(License.disableStat);
                    writer.Write(License.disablePlugin);
                    writer.Write(License.disableDraw);
                    writer.Write(License.ExpirationDate.ToBinary());

                    writer.Write(UsbSerial ?? string.Empty);

                    return ms.ToArray();
                }
            }

            public static LicenseContainer Deserialize(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                using (var reader = new BinaryReader(ms, Encoding.UTF8))
                {
                    return new LicenseContainer
                    {
                        License = new LicenseData
                        {
                            //TODO поменять
                            UserName = reader.ReadString(),
                            UserProfile = reader.ReadString(),
                            disableStat = reader.ReadBoolean(),
                            disablePlugin = reader.ReadBoolean(),
                            disableDraw = reader.ReadBoolean(),
                            ExpirationDate = DateTime.FromBinary(reader.ReadInt64())
                        },
                        UsbSerial = reader.ReadString()
                    };
                }
            }
        }

        public class LicenseKeyGenerator
        {
            private const string EncryptionSalt = "someSalt";

            public byte[] GenerateKey(LicenseData data, string usbSerial)
            {
                var container = new LicenseContainer
                {
                    License = data,
                    UsbSerial = usbSerial
                };

                byte[] serializedData = container.Serialize();

                byte[] hash;
                using (var sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(serializedData);
                }

                byte[] encryptedData;
                byte[] iv;
                using (var aes = Aes.Create())
                {
                    using (var deriveBytes = new Rfc2898DeriveBytes(usbSerial, Encoding.UTF8.GetBytes(EncryptionSalt), 1000))
                    {
                        aes.Key = deriveBytes.GetBytes(32);
                    }

                    aes.GenerateIV();
                    iv = aes.IV;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(serializedData, 0, serializedData.Length);
                        }
                        encryptedData = ms.ToArray();
                    }
                }

                var result = new byte[iv.Length + encryptedData.Length + hash.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedData, 0, result, iv.Length, encryptedData.Length);
                Buffer.BlockCopy(hash, 0, result, iv.Length + encryptedData.Length, hash.Length);

                return result;
            }
        }
    }
}
