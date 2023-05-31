using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;

namespace myencrypter2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void addFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Allow multiple file selection
            openFileDialog.Multiselect = true;

            // Set initial directory (optional)
            openFileDialog.InitialDirectory = "C:\\";

            // Set file filters (optional)
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Show the dialog and get the result
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Get an array of selected file paths
                string[] selectedFilePaths = openFileDialog.FileNames;

                // Optionally display a completion message
                MessageBox.Show("Files encrypted successfully.");
            }
        }


        private List<string> GetFilePathToEncrypt()
        {
            List<string> filePaths = new List<string>();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePaths.AddRange(openFileDialog.FileNames);
            }

            return filePaths;
        }
        async void encryptFile_Click(object sender, EventArgs e)
        {
            // Get the selected encryption algorithm


            // Get the file path to encrypt
            List<string> filePath = GetFilePathToEncrypt();

            if (filePath == null || filePath.Count == 0)
            {
                // List is null or empty
                // Handle the case here
            }
            else
            {
                // List is not null or empty
                // Process the file paths here


                try
                {
                    // Check which encryption option is selected
                    if (radioButton1.Checked)
                    {
                        await EncryptAES();
                        return EncryptAES;
                        // Perform encryption using AES
                        // Your AES encryption logic here
                    }
                    else if (radioButton2.Checked)
                    {
                        await EncryptDES();
                        return EncryptDES;
                        // Perform encryption using DES
                        // Your DES encryption logic here
                    }
                    else if (radioButton3.Checked)
                    {
                        await EncryptTripleDES();
                        return EncryptDES;
                        // Perform encryption using TripleDES
                        // Your TripleDES encryption logic here
                    }
                    else if (radioButton4.Checked)
                    {
                        await EncryptRSA();
                        return EncryptRSA;
                        // Perform encryption using RSA
                        // Your RSA encryption logic here
                    }
                    else if (radioButton5.Checked)
                    {
                        await EncryptWithKey();
                        return EncryptWithKey;
                        // Perform encryption using ECC
                        // Your ECC encryption logic here
                    }
                    else
                    {
                        // No encryption option selected
                        MessageBox.Show("Please select an encryption option.");
                    }
                }
                catch (Exception ex) { }

            }
        }
        private AsymmetricCipherKeyPair GenerateECCKeyPair()
        {
            // Generate ECC key pair using Bouncy Castle
            ECKeyPairGenerator generator = new ECKeyPairGenerator();
            X9ECParameters curve = NistNamedCurves.GetByName("P-256"); // Choose the desired curve
            ECDomainParameters domainParameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            ECKeyGenerationParameters keyGenerationParameters = new ECKeyGenerationParameters(domainParameters, new SecureRandom());
            generator.Init(keyGenerationParameters);
            AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

            return keyPair;
        }

        private void EncryptECC()
        {
            try
            {
                // Generate ECC key pair
                AsymmetricCipherKeyPair keyPair = GenerateECCKeyPair();

                // Convert the key pair to appropriate parameters
                ECPublicKeyParameters publicKey = (ECPublicKeyParameters)keyPair.Public;
                ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)keyPair.Private;

                // Get the plain text from the user (e.g., from a TextBox)
                //string plainText = textBox1.Text;

                // Encode the plain text as bytes
                // byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                // Create the ECC encryptor using the public key
                IBufferedCipher encryptor = CipherUtilities.GetCipher("ECIES");
                encryptor.Init(true, publicKey);

                // Encrypt the plain text
                // byte[] cipherTextBytes = encryptor.DoFinal(plainTextBytes);

                // Convert the cipher text to a readable string (e.g., Base64)
                // string cipherText = Convert.ToBase64String(cipherTextBytes);

                // Display the encrypted result (e.g., in a Label or TextBox)
                //textBox2.Text = cipherText;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during encryption
                Console.WriteLine("Encryption Error: " + ex.Message);
            }
        }

        private byte[] EncryptWithKey(byte[] data, byte[] key)
        {
            // Use any symmetric encryption algorithm to encrypt the data
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                using (MemoryStream ms = new MemoryStream())
                {
                    // Write the IV to the output stream
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // Create a CryptoStream to perform encryption
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Write the encrypted data to the CryptoStream
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    // Return the encrypted data
                    return ms.ToArray();
                }
            }
        }
    
        private byte[] EncryptDES(byte[] data, byte[] key, byte[] iv)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = key;
                des.IV = iv;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private byte[] EncryptTripleDES(byte[] data, byte[] key, byte[] iv)
        {
            using (TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider())
            {
                tripleDES.Key = key;
                tripleDES.IV = iv;
                tripleDES.Mode = CipherMode.CBC;
                tripleDES.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private byte[] EncryptRSA(byte[] data, RSAParameters publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(publicKey);

                // Encrypt the data using RSA encryption
                byte[] encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

                return encryptedData;
            }
        }

        private byte[] EncryptAES(byte[] data, byte[] key, byte[] iv)
        {
            using (AesManaged aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;

                // Create an encryptor to perform the AES encryption
                ICryptoTransform encryptor = aes.CreateEncryptor();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        // Write the encrypted data to the memory stream
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    // Return the encrypted data as a byte array
                    return memoryStream.ToArray();
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

    }

   