using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TicTacToe_Platform.Models.Configurations;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Helpers
{
    public class CryptoUtility
    {
        private readonly SignUpConfiguration _signUpConfiguration;
        private readonly AuthorizationConfiguration _authConfiguration; 

        public CryptoUtility(AuthorizationConfiguration authConfiguration, SignUpConfiguration signUpConfiguration)
        {
            _authConfiguration = authConfiguration;
            _signUpConfiguration = signUpConfiguration;
        }

        public string? EncryptObject<T>(T obj) where T : class
        {
            try
            {
                return EncryptString(JsonConvert.SerializeObject(obj));
            }
            catch
            {
                return null;
            }
        }

        public T? DecryptObject<T>(string encryptedObject) where T : class
        {
            try
            {
                
                return encryptedObject is null ? null : JsonConvert.DeserializeObject<T>(DecryptString(encryptedObject));
            }
            catch
            {
                return null;
            }
        }

        public string EncryptString(string text)
        {
            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(new Guid(_authConfiguration.Key).ToByteArray(), aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, _authConfiguration.IvSize);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(text);

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        public string? DecryptString(string cipherText)
        {
            try
            {
                cipherText = cipherText?.Replace(" ", "+");
                
                byte[] bytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    var iv = new byte[_authConfiguration.IvSize];
                    memoryStream.Read(iv, 0, _authConfiguration.IvSize);

                    aes.Key = new Guid(_authConfiguration.Key).ToByteArray();
                    aes.IV = iv;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                        return streamReader.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        public string ShaMacEncryptString(string value)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_signUpConfiguration.Key);

            using var hmac = new HMACSHA256(keyBytes);
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));

            return Convert.ToBase64String(bytes);
        }
        public IUserModel ShaMacEncryptUserData(IUserModel model) => ShaMacEncryptUserData(model.Login, model.Password);
        public IUserModel ShaMacEncryptUserData(string login, string password)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_signUpConfiguration.Key);

            using var hmac = new HMACSHA256(keyBytes);
            var loginBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(login));
                
            var passwordBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return new SignUpModel
            {
                Login = Convert.ToBase64String(loginBytes),
                Password = Convert.ToBase64String(passwordBytes)
            };
        }

    }
}
