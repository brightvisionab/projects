using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections;

namespace BrightVision.Common
{
    public static class SecurityUtility
    {
        public  enum RSACSPKey { PrivateKey, PublicKey };
        public static Dictionary<RSACSPKey, string> GenKeys()
        {
            RSACryptoServiceProvider RSAcsp = new RSACryptoServiceProvider(2048);
            string privateKey = "<BitStrength>2048</BitStrength>" + RSAcsp.ToXmlString(true);
            string publicKey = "<BitStrength>2048</BitStrength>" + RSAcsp.ToXmlString(false);
            var dic = new Dictionary<RSACSPKey, string>();
            dic.Add(RSACSPKey.PrivateKey, privateKey);
            dic.Add(RSACSPKey.PublicKey, privateKey);
            return dic;
        }

        public static string Encrypt(string inputString, string xmlString)
        {
            //string FullKey = Key.Substring(31);
            ////MessageBox.Show(FullKey);
            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            //rsa.FromXmlString(FullKey);
            //byte[] byteData = Encoding.UTF32.GetBytes(text);
            //int maxLength = 214;
            //int dataLength = byteData.Length;
            //int iterations = dataLength / maxLength;

            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i <= iterations; i++)
            //{
            //    byte[] tempBytes = new byte[(dataLength - maxLength * i > maxLength) ? maxLength : dataLength - maxLength * i];
            //    Buffer.BlockCopy(byteData, maxLength * i, tempBytes, 0, tempBytes.Length);

            //    byte[] EncbyteData = rsa.Encrypt(tempBytes, false);
            //    sb.Append(Convert.ToBase64String(EncbyteData));
            //}
            //return sb.ToString();
            // TODO: Add Proper Exception Handlers
            int dwKeySize = 1024;
            RSACryptoServiceProvider rsaCryptoServiceProvider = 
                                          new RSACryptoServiceProvider( dwKeySize );
            rsaCryptoServiceProvider.FromXmlString( xmlString );
            int keySize = dwKeySize / 8;
            byte[] bytes = Encoding.UTF32.GetBytes( inputString );
            // The hash function in use by the .NET RSACryptoServiceProvider here 
            // is SHA1
            // int maxLength = ( keySize ) - 2 - 
            //              ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
            int maxLength = keySize - 42;
            int dataLength = bytes.Length;
            int iterations = dataLength / maxLength;
            StringBuilder stringBuilder = new StringBuilder();
            for( int i = 0; i <= iterations; i++ )
            {
                byte[] tempBytes = new byte[ 
                        ( dataLength - maxLength * i > maxLength ) ? maxLength : 
                                                      dataLength - maxLength * i ];
                Buffer.BlockCopy( bytes, maxLength * i, tempBytes, 0, 
                                  tempBytes.Length );
                byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt( tempBytes,
                                                                          true );
                // Be aware the RSACryptoServiceProvider reverses the order of 
                // encrypted bytes. It does this after encryption and before 
                // decryption. If you do not require compatibility with Microsoft 
                // Cryptographic API (CAPI) and/or other vendors. Comment out the 
                // next line and the corresponding one in the DecryptString function.
                Array.Reverse( encryptedBytes );
                // Why convert to base 64?
                // Because it is the largest power-of-two base printable using only 
                // ASCII characters
                stringBuilder.Append( Convert.ToBase64String( encryptedBytes ) );
            }
            return stringBuilder.ToString();

        }

        public static string Decrypt(string inputString, int strength, string xmlString)
        {
            //string FullKey = Key.Substring(31);
            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            //rsa.FromXmlString(FullKey);
            //int base64BlockSize = (256 % 3 != 0) ? ((256 / 3) * 4) + 4 : (256 / 3) * 4;
            //int iterations = text.Length / base64BlockSize;
            ////ArrayList al = new ArrayList();
            //string ptext = "";
            //int l = 0;
            //byte[] fullbytes = new byte[0];
            //for (int i = 0; i < iterations; i++)
            //{
            //    byte[] encBytes = Convert.FromBase64String(text.Substring(base64BlockSize * i, base64BlockSize));
            //    byte[] bytes = rsa.Decrypt(encBytes, false);
            //    //ptext += Encoding.UTF32.GetString(bytes);
            //    //al.Add(bytes);
            //    Array.Resize(ref fullbytes, fullbytes.Length + bytes.Length);
            //    for (int k = 0; k < bytes.Length; k++)
            //    {
            //        fullbytes[l] = bytes[k];
            //        l++;
            //    }
            //}
            //  return ptext;

            int dwKeySize = 1024;
            //ptext = Encoding.UTF32.GetString(fullbytes);
            // TODO: Add Proper Exception Handlers
            RSACryptoServiceProvider rsaCryptoServiceProvider
                                     = new RSACryptoServiceProvider(dwKeySize);

            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ?
              (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
            int iterations = inputString.Length / base64BlockSize;
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < iterations; i++)
            {
                byte[] encryptedBytes = Convert.FromBase64String(
                     inputString.Substring(base64BlockSize * i, base64BlockSize));
                // Be aware the RSACryptoServiceProvider reverses the order of 
                // encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic 
                // API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the 
                // EncryptString function.
                Array.Reverse(encryptedBytes);
                arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(
                                    encryptedBytes, true));
            }
            return Encoding.UTF32.GetString(arrayList.ToArray(
                                      Type.GetType("System.Byte")) as byte[]);
          
        }
    }
}
