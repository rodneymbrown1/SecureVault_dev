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
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using Org.BouncyCastle.Utilities;

namespace myencrypter2
{
    public partial class Form1 : Form
    {
        //private AesClass _AES;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private List<string> selectedFilePaths = new List<string>();

        private void addFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Allow multiple file selection
            openFileDialog.Multiselect = true;

            // Set initial directory (optional)
            openFileDialog.InitialDirectory = "C:\\";

            // Set file filters (optional)
            /*openFileDialog.Filter = "All files (*.*)|*.*";*/

            // Show the dialog and get the result
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Get an array of selected file paths
                selectedFilePaths.AddRange(openFileDialog.FileNames);
                string selectedFilesString = string.Join(", ", selectedFilePaths);
                // Optionally display a completion message
                Debug.WriteLine("Files added successfully: " + selectedFilesString);
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
        private void encryptFile_Click(object sender, EventArgs e)
        {
            if (selectedFilePaths == null || selectedFilePaths.Count == 0)
            {
                // List is null or empty
                // Handle the case here
            }
            else
            {
                
                    try
                    {
                        var FilePath = GetFilePathToEncrypt();
                        var FileBytes = ConvertToBytes(FilePath);
                       
                        // Check which encryption option is selected
                        if (radioButton1.Checked)
                    {   // Generate the key and IV
                        (byte[] _key, byte[] iv) = AesClass.GenerateAESKeyAndIV();

                        var (_encryptedBytes, _iv) = AesClass.EncryptAES(FileBytes, _key, iv);
                        
                        SaveToFile(selectedFilePaths, selectedFilePaths, selectedFilePaths, _key, _iv, _encryptedBytes);
                        }
                        else if (radioButton2.Checked)
                        {
                            /*await EncryptDES();
                            return EncryptDES;*/
                            // Perform encryption using DES
                            // Your DES encryption logic here
                        }
                        else if (radioButton3.Checked)
                        {
                            /*await EncryptTripleDES();
                            return EncryptDES;*/
                            // Perform encryption using TripleDES
                            // Your TripleDES encryption logic here
                        }
                        else if (radioButton4.Checked)
                        {
                            /*await EncryptRSA();
                            return EncryptRSA;*/
                            // Perform encryption using RSA
                            // Your RSA encryption logic here
                        }
                        else if (radioButton5.Checked)
                        {
                            //EncryptECC(selectedFilePaths);
                            /*eturn EncryptECC;*/
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
        /*        private AsymmetricCipherKeyPair GenerateECCKeyPair()
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
                */
        public byte[] ConvertToBytes(List<string> filePaths)
        {
            List<byte> byteList = new List<byte>();

            foreach (string filePath in filePaths)
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byteList.AddRange(fileBytes);
            }

            byte[] bytes = byteList.ToArray();
            return bytes;
        }
        /*
                private void EncryptECC(List<string> filePaths)
                {
                    try
                    {
                        // Generate ECC key pair
                        AsymmetricCipherKeyPair keyPair = GenerateECCKeyPair();

                        // Convert the key pair to appropriate parameters
                        ECPublicKeyParameters publicKey = (ECPublicKeyParameters)keyPair.Public;
                        ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)keyPair.Private;
                        Debug.WriteLine("From EncryptECC: keypair: "+keyPair.ToString());
                        byte[] FileBytes = ConvertToBytes(filePaths);
                        Debug.WriteLine("From EncryptECC: FileBytes: " + FileBytes.ToString());
                        // Create the ECC encryptor using the public key
                        IBufferedCipher encryptor = CipherUtilities.GetCipher("ECB");
                        encryptor.Init(true, publicKey);

                        // Encrypt the File Bytes
                        byte[] cipherFileBytes = encryptor.DoFinal();
                        Debug.WriteLine("From EncryptECC, Encrypted File Bytes: " + cipherFileBytes);
                        // Convert the File Bytes to a readable string (e.g., Base64)
                        // Convert.ToBase64String(cipherFileBytes);

                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that may occur during encryption
                        Debug.WriteLine("Encryption Error: " + ex.Message);
                    }
                }
        */

        /*        private byte[] EncryptDES(byte[] data, byte[] key, byte[] iv)
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
        */
        /*        private byte[] EncryptTripleDES(byte[] data, byte[] key, byte[] iv)
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
        */
        /*        private byte[] EncryptRSA(byte[] data, RSAParameters publicKey)
                {
                    using (RSA rsa = RSA.Create())
                    {
                        rsa.ImportParameters(publicKey);

                        // Encrypt the data using RSA encryption
                        byte[] encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

                        return encryptedData;
                    }
                }
        */

        public static void SaveToFile(List<string> publicKeyFilePaths, List<string> ivFilePaths, List<string> ciphertextFilePaths, byte[] publicKey, byte[] iv, byte[] ciphertext)
        {
            for (int i = 0; i < publicKeyFilePaths.Count; i++)
            {
                string publicKeyFilePath = publicKeyFilePaths[i];
                string ivFilePath = ivFilePaths[i];
                string ciphertextFilePath = ciphertextFilePaths[i];

                // Change the file extensions as desired
                string publicKeyFile = publicKeyFilePath + ".key";
                string ivFile = ivFilePath + ".iv";
                string ciphertextFile = ciphertextFilePath + ".file";

                File.WriteAllBytes(publicKeyFile, publicKey);
                File.WriteAllBytes(ivFile, iv);
                File.WriteAllBytes(ciphertextFile, ciphertext);
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
        private void DecrpytECC(string encryptedFilePath, AsymmetricKeyParameter privateKey, string outputFilePath)
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
                IBufferedCipher cipher = CipherUtilities.GetCipher("ECIES");
                cipher.Init(false, privateKey);
                byte[] decryptedData = cipher.DoFinal(encryptedData);
                File.WriteAllBytes(outputFilePath, decryptedData);
                Debug.WriteLine("ECC decryption completed. Decrypted file saved to: " + outputFilePath);


            }

            catch (Exception ex) { Debug.WriteLine("Decryption error: " + ex.Message); }

        }
    }

   class AesClass
    {
        // Create a new instance of the AesCryptoServiceProvider
        // class.  This generates a new key and initialization
        // vector (IV).

        public static (byte[] key, byte[] iv) GenerateAESKeyAndIV()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();

                byte[] key = aesAlg.Key;
                byte[] iv = aesAlg.IV;

                return (key, iv);
            }
        }

 /*       public static (byte[] fileBytes, byte[] iv, byte[] publicKey) EncryptAESforEach(byte[] fileBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();

                byte[] key = aesAlg.Key;
                byte[] iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, iv);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                    }

                    byte[] encryptedBytes = msEncrypt.ToArray();

                    // Return the encrypted bytes, IV, and public key as a tuple
                    return (encryptedBytes, iv, key);
                }
            }
        }
*/
        public static (byte[] encryptedBytes, byte[] iv) EncryptAES(byte[] fileBytes, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, iv);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                    }

                    byte[] encryptedBytes = msEncrypt.ToArray();

                    // Return the encrypted bytes and IV as a tuple
                    return (encryptedBytes, iv);
                }
            }
        }


        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
            {
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");

                // Declare the string used to hold
                // the decrypted text.
                string plaintext = null;

                // Create an AesCryptoServiceProvider object
                // with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plaintext;
            }
        
    }
}

   