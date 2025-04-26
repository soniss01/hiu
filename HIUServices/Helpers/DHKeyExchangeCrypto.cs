using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;
using Encoder = Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace HIUServices.Helpers
{
    public static class DHKeyExchangeCrypto
    {
        public static readonly string CURVE = "curve25519";
        public static readonly string ALGORITHM = "ECDH";

        // Encrypts a string using ECDH for key exchange and AES/GCM for encryption
        public static string EncryptString(string stringToEncrypt, byte[] xorOfRandoms, string senderPrivateKey, string receiverPublicKey)
        {
            // Generate the shared secret key
            var sharedKey = GetBase64FromByte(GetSharedSecretValue(senderPrivateKey, receiverPublicKey));
            Console.WriteLine("SHARED SECRET (DHK): " + sharedKey);
            Console.WriteLine("\n");

            // Generate salt and IV
            var salt = xorOfRandoms.Take(20).ToArray();

            //Last 12 byte of XOR
            var iv = xorOfRandoms.Skip(xorOfRandoms.Length - 12).ToArray();

            //AES Encryption Key Generation
            var aesKey = GenerateAesKey(sharedKey, salt);
            Console.WriteLine("HKDF AES KEY: " + GetBase64FromByte(aesKey.ToArray()));
            Console.WriteLine("\n");
            // Encrypt the data
            var encryptedString = string.Empty;
            try
            {
                var dataBytes = Encoding.UTF8.GetBytes(stringToEncrypt);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(aesKey.ToArray()), 128, iv, null);
                cipher.Init(true, parameters);
                var encryptedBytes = new byte[cipher.GetOutputSize(dataBytes.Length)];
                var length = cipher.ProcessBytes(dataBytes, 0, dataBytes.Length, encryptedBytes, 0);
                cipher.DoFinal(encryptedBytes, length);
                encryptedString = Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return encryptedString;
        }

        // Decrypts a string using ECDH for key exchange and AES/GCM for decryption
        public static string DecryptString(string stringToDecrypt, byte[] xorOfRandoms, string receiverPrivateKey, string senderPublicKey)
        {
            // Generate the shared key
            var sharedKey = GetBase64FromByte(GetSharedSecretValue(receiverPrivateKey, senderPublicKey));
            Console.WriteLine("DHK SHARED SECRET: " + sharedKey);
            Console.WriteLine("\n");

            // Generate salt, IV and aes key
            var salt = xorOfRandoms.Take(20).ToArray();

            //Last 12 byte of XOR
            var iv = xorOfRandoms.Skip(xorOfRandoms.Length - 12).ToArray();

            //AES Encryption Key Generation
            var aesKey = GenerateAesKey(sharedKey, salt);
            Console.WriteLine("HKDF AES KEY: " + GetBase64FromByte(aesKey.ToArray()));
            Console.WriteLine("\n");
            // Decrypt the data
            string decryptedData = "";
            try
            {
                var dataBytes = Convert.FromBase64String(stringToDecrypt);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(aesKey.ToArray()), 128, iv, null);
                cipher.Init(false, parameters);
                var plainBytes = new byte[cipher.GetOutputSize(dataBytes.Length)];
                var length = cipher.ProcessBytes(dataBytes, 0, dataBytes.Length, plainBytes, 0);
                cipher.DoFinal(plainBytes, length);
                decryptedData = Encoding.UTF8.GetString(plainBytes).TrimEnd('\0');
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return decryptedData;
        }

        // Generates a key pair using ECDH
        public static AsymmetricCipherKeyPair GenerateKey()
        {
            var ecParams = CustomNamedCurves.GetByName(CURVE);
            var ecSpec = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H, ecParams.GetSeed());
            var generator = GeneratorUtilities.GetKeyPairGenerator(ALGORITHM);
            generator.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
            return generator.GenerateKeyPair();
        }

        // Generates the shared secret value from private and public keys
        public static byte[] GetSharedSecretValue(string privKey, string pubKey)
        {
            var privateKey = GetPrivateKeyFrom(privKey);
            var publicKey = GetPublicKeyFrom(pubKey);
            var agreement = AgreementUtilities.GetBasicAgreement(ALGORITHM);
            agreement.Init(privateKey);
            var result = agreement.CalculateAgreement(publicKey);
            return result.ToByteArrayUnsigned();
        }

        // XORs two byte arrays
        public static byte[] XorOfRandom(byte[] randomKeySender, byte[] randomKeyReceiver)
        {
            var result = new byte[randomKeyReceiver.Length];
            for (var i = 0; i < randomKeySender.Length; i++)
            {
                result[i] = (byte)(randomKeySender[i] ^ randomKeyReceiver[i % randomKeyReceiver.Length]);
            }
            return result;
        }

        // Generates a 32-byte random key
        public static string GenerateRandomKey()
        {
            var randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return GetBase64FromByte(randomBytes);
        }

        // Generates an AES key using HKDF
        public static byte[] GenerateAesKey(string sharedKey, byte[] salt)
        {
            var hkdf = new HkdfBytesGenerator(new Sha256Digest());
            var hkdfParams = new HkdfParameters(Convert.FromBase64String(sharedKey), salt, null);
            hkdf.Init(hkdfParams);
            var aesKey = new byte[32];
            hkdf.GenerateBytes(aesKey, 0, aesKey.Length);
            return aesKey;
        }

        // Converts byte array to Base64 string
        public static string GetBase64FromByte(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        // Converts Base64 string to byte array
        public static byte[] GetByteFromBase64(string value)
        {
            return Convert.FromBase64String(value);
        }

        // Converts public key to string
        public static string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var publicKey = (ECPublicKeyParameters)keyPair.Public;
            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            return Convert.ToBase64String(publicKeyInfo.GetEncoded());
        }

        // Converts private key to string
        public static string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var privateKey = (ECPrivateKeyParameters)keyPair.Private;
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            return GetBase64FromByte(privateKeyInfo.ToAsn1Object().GetDerEncoded());
        }

        // Converts string to private key
        public static AsymmetricKeyParameter GetPrivateKeyFrom(string privateKey)
        {
            return PrivateKeyFactory.CreateKey(GetByteFromBase64(privateKey));
        }

        // Converts string to public key
        public static AsymmetricKeyParameter GetPublicKeyFrom(string publicKey)
        {
            return PublicKeyFactory.CreateKey(GetByteFromBase64(publicKey));
        }
    }

    //public static class DHKeyExchangeCrypto
    //{

    //    public static readonly string CURVE = "curve25519";
    //    public static readonly string ALGORITHM = "ECDH";

    //    // Method for encrypting the string
    //    public static string EncryptString(string stringToEncrypt,
    //                                byte[] xorOfRandoms,
    //                                string senderPrivateKey,
    //                                string receiverPublicKey)
    //    {
    //        // Generating the shared key using the parameters available
    //        var sharedKey = GetBase64FromByte(GetSharedSecretValue(senderPrivateKey, receiverPublicKey));
    //        //Console.WriteLine("DHE SHARED SECRET: " + sharedKey);

    //        // Generate the salt and IV
    //        var salt = xorOfRandoms.Take(20);
    //        var iv = xorOfRandoms.TakeLast(12);
    //        var aesKey = GenerateAesKey(sharedKey, salt);
    //        //Console.WriteLine("HKDF AES KEY: " + GetBase64FromByte(aesKey.ToArray()));

    //        // Encrypt the data
    //        var encryptedString = string.Empty;
    //        try
    //        {
    //            var dataBytes = Encoding.UTF8.GetBytes(stringToEncrypt);
    //            var cipher = new GcmBlockCipher(new AesEngine());
    //            var parameters =
    //                new AeadParameters(new KeyParameter(aesKey.ToArray()), 128, iv.ToArray(), null);
    //            cipher.Init(true, parameters);
    //            var encryptedBytes = new byte[cipher.GetOutputSize(dataBytes.Length)];
    //            var returnLengthEncryptedData = cipher.ProcessBytes
    //                (dataBytes, 0, dataBytes.Length, encryptedBytes, 0);
    //            cipher.DoFinal(encryptedBytes, returnLengthEncryptedData);
    //            encryptedString = Convert.ToBase64String(encryptedBytes, Base64FormattingOptions.None);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Write(ex.Message);
    //        }

    //        // Return the encrypted string
    //        return encryptedString;
    //    }

    //    // Method for decrypting the string
    //    public static string DecryptString(string stringToDecrypt,
    //        byte[] xorOfRandoms,
    //        string receiverPrivateKey,
    //        string senderPublicKey)
    //    {
    //        // Generating the shared key using the parameters available
    //        var sharedKey = GetBase64FromByte(GetSharedSecretValue(receiverPrivateKey, senderPublicKey));
    //        //Console.WriteLine("DHE SHARED SECRET: " + sharedKey);

    //        // Generate the salt, IV and aes key
    //        var salt = xorOfRandoms.Take(20);
    //        var iv = xorOfRandoms.TakeLast(12);
    //        var aesKey = GenerateAesKey(sharedKey, salt);
    //        //Console.WriteLine("HKDF AES KEY: " + GetBase64FromByte(aesKey.ToArray()));

    //        // Decrypting the data
    //        String decryptedData = "";
    //        try
    //        {
    //            var dataBytes = GetByteFromBase64(stringToDecrypt).ToArray();
    //            var cipher = new GcmBlockCipher(new AesEngine());
    //            var parameters =
    //                new AeadParameters(new KeyParameter(aesKey.ToArray()), 128, iv.ToArray(), null);
    //            cipher.Init(false, parameters);
    //            byte[] plainBytes = new byte[cipher.GetOutputSize(dataBytes.Length)];
    //            int retLen = cipher.ProcessBytes
    //                (dataBytes, 0, dataBytes.Length, plainBytes, 0);
    //            cipher.DoFinal(plainBytes, retLen);
    //            decryptedData = Encoding.UTF8.GetString(plainBytes);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Write(ex);
    //        }

    //        // Returning decrypted data
    //        return decryptedData;
    //    }

    //    // Generating DHE Key
    //    public static AsymmetricCipherKeyPair GenerateKey()
    //    {
    //        var ecP = CustomNamedCurves.GetByName(CURVE);
    //        var ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H, ecP.GetSeed());
    //        var generator = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator(ALGORITHM);
    //        generator.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
    //        return generator.GenerateKeyPair();
    //    }

    //    // Generating shared key
    //    public static byte[] GetSharedSecretValue(string privKey, string pubKey)
    //    {
    //        var privateKey = GetPrivateKeyFrom(privKey);
    //        var publicKey = GetPublicKeyFrom(pubKey);
    //        var agreement = AgreementUtilities.GetBasicAgreement(ALGORITHM);
    //        agreement.Init(privateKey);
    //        var result = agreement.CalculateAgreement(publicKey);
    //        return result.ToByteArrayUnsigned();
    //    }

    //    // XOR of teo random string (sender and receiver)
    //    public static IEnumerable<byte> XorOfRandom(string randomKeySender, string randomKeyReceiver)
    //    {
    //        var randomKeySenderBytes = GetByteFromBase64(randomKeySender).ToArray();
    //        var randomKeyReceiverBytes = GetByteFromBase64(randomKeyReceiver).ToArray();
    //        var sb = new byte[randomKeyReceiverBytes.Length];
    //        for (var i = 0; i < randomKeySenderBytes.Length; i++)
    //        {
    //            sb[i] = (byte)(randomKeySenderBytes[i] ^ randomKeyReceiverBytes[i % randomKeyReceiverBytes.Length]);
    //        }

    //        return sb;
    //    }

    //    // Generate 32 byte random Key 
    //    public static string GenerateRandomKey()
    //    {
    //        var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
    //        var randomBytes = new byte[32];
    //        rngCryptoServiceProvider.GetBytes(randomBytes);
    //        return GetBase64FromByte(randomBytes);
    //    }

    //    // Method for getting Aes key using HKDF
    //    public static IEnumerable<byte> GenerateAesKey(string sharedKey, IEnumerable<byte> salt)
    //    {
    //        var hkdfBytesGenerator = new HkdfBytesGenerator(new Sha256Digest());
    //        var hkdfParameters = new HkdfParameters(GetByteFromBase64(sharedKey).ToArray(),
    //            salt.ToArray(),
    //            null);
    //        hkdfBytesGenerator.Init(hkdfParameters);
    //        var aesKey = new byte[32];
    //        hkdfBytesGenerator.GenerateBytes(aesKey, 0, 32);
    //        return aesKey;
    //    }

    //    // Get base64 from byte array.
    //    public static string GetBase64FromByte(IEnumerable<byte> value)
    //    {
    //        return Encoder.Base64.ToBase64String((byte[])value);
    //    }

    //    //Get byte array from string.
    //    public static IEnumerable<byte> GetByteFromBase64(string value)
    //    {
    //        return Encoder.Base64.Decode(value);
    //    }

    //    // Converting Public key to string
    //    public static string GetPublicKey(AsymmetricCipherKeyPair keyPair)
    //    {
    //        return Convert.ToBase64String(Org.BouncyCastle.X509.SubjectPublicKeyInfoFactory
    //            .CreateSubjectPublicKeyInfo(keyPair.Public).GetEncoded());
    //    }

    //    // Converting Private key to string
    //    public static string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
    //    {
    //        var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
    //        var encoded = keyInfo.ToAsn1Object().GetDerEncoded();
    //        return GetBase64FromByte(encoded);
    //    }

    //    // Converting string to privateKey
    //    public static AsymmetricKeyParameter GetPrivateKeyFrom(string privateKey)
    //    {
    //        return PrivateKeyFactory.CreateKey((byte[])GetByteFromBase64(privateKey));
    //    }

    //    // Converting string to publicKey
    //    public static AsymmetricKeyParameter GetPublicKeyFrom(string publicKey)
    //    {
    //        return PublicKeyFactory.CreateKey((byte[])GetByteFromBase64(publicKey));
    //    }
    //}

}
